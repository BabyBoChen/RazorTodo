using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using Newtonsoft.Json;
using RazorTodo.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using RazorTodo.Abstraction.Models;

namespace RazorTodo.Service
{
    public class DropboxService : ICloudDriveService
    {
        private static string TokenPath { get; set; }
        private static string RootFolderName { get; set; }
        private static bool _IsRegister = false;
        public static void Register(string tokenPath, string rootFolderName)
        {
            TokenPath = tokenPath;
            if (rootFolderName.Contains("/"))
            {
                throw new Exception("DropboxService.Register: rootFolderName contains illegal char");
            }
            RootFolderName = rootFolderName;
            _IsRegister = true;
        }
        
        private DropboxClient _Client;
        private DropboxClient Client 
        { 
            get
            {
                if(this._Client == null)
                {
                    if (string.IsNullOrWhiteSpace(_Token))
                    {
                        _Token = this.RefreshToken();
                    }
                    this._Client = new DropboxClient(_Token);
                }
                Task.Run(async () =>
                {
                    try
                    {
                        var res = await this._Client.Check.UserAsync();
                    }
                    catch (Exception)
                    {
                        _Token = this.RefreshToken();
                        this._Client = new DropboxClient(_Token);
                    }
                }).Wait();
                return this._Client;
            }
        }
        private static string _Token { get; set; }
        private static Dictionary<string, string> _CachedSharedLink { get; } = new Dictionary<string, string>();

        public DropboxService()
        {
            if(!_IsRegister)
            {
                throw new Exception("This service has not been registered yet. Please invoke DropboxService.Register before instantiating this service.");
            }
        }

        private string RefreshToken()
        {
            string newAccessToken = "";
            string refreshTokenJson = File.ReadAllText(TokenPath);
            var refreshToken = JsonConvert.DeserializeObject<DropboxToken>(refreshTokenJson);
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), $"https://api.dropbox.com/oauth2/token"))
                {
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{refreshToken.AppKey}:{refreshToken.AppSecret}"));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    var contentList = new List<string>();
                    contentList.Add($"refresh_token={refreshToken.RefreshToken}");
                    contentList.Add("grant_type=refresh_token");
                    request.Content = new StringContent(string.Join("&", contentList));
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var res = httpClient.SendAsync(request).Result;
                    string body = res.Content.ReadAsStringAsync().Result;
                    refreshToken = JsonConvert.DeserializeObject<DropboxToken>(body);
                    newAccessToken = refreshToken.AccessToken;
                }
            }
            return newAccessToken;
        }

        public class DropboxToken
        {
            [JsonProperty("app_key")]
            public string AppKey { get; set; }

            [JsonProperty("app_secret")]
            public string AppSecret { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
        }

        public List<CloudDriveEntry> GetFilesInFolder(params string[] folderPaths)
        {
            List<CloudDriveEntry> files = new List<CloudDriveEntry>();
            List<Metadata> fileMetas = new List<Metadata>();
            string fullPath = this.JoinPaths(folderPaths);
            Task.Run(async () => 
            {
                var folder = await this.GetFolder(fullPath);
                if(folder != null)
                {
                    var res = await this.Client.Files.ListFolderAsync(fullPath);
                    fileMetas.AddRange(res.Entries);
                    while(res.HasMore)
                    {
                        res = await this.Client.Files.ListFolderContinueAsync(res.Cursor);
                        fileMetas.AddRange(res.Entries);
                    }
                }
            }).Wait();
            for(int i = 0; i < fileMetas.Count; i++)
            {
                Metadata meta = fileMetas[i];
                if (meta.IsFile)
                {
                    var f = new CloudDriveFile();
                    f.FileId = meta.AsFile.Id;
                    f.Filename = meta.Name;
                    string[] fileFullPaths = null;
                    if (folderPaths != null && folderPaths.Length > 0)
                    {
                        var ls = folderPaths.ToList();
                        ls.Add(f.Filename);
                        fileFullPaths = ls.ToArray();
                    }
                    else
                    {
                        fileFullPaths = new string[1] { f.Filename };
                    }
                    f.SharedLink = this.GetSharedLink(fileFullPaths);
                    files.Add(f);
                }
                else if(meta.IsFolder)
                {
                    var f = new CloudDriveFolder();
                    f.FileId = meta.AsFolder.Id;
                    f.FolderName = meta.AsFolder.Name;
                    f.SharedLink = null;
                    files.Add(f);
                }
            }
            return files;
        }

        public string CreateFolder(string folderName, params string[] parentFolderPaths)
        {
            string folderId = null;
            string fullPath = $"/{RootFolderName}";
            if (parentFolderPaths != null)
            {
                fullPath = this.JoinPaths(parentFolderPaths) + $"/{folderName}";
            }
            Task.Run(async () => 
            {
                Metadata folder = await this.GetFolder(fullPath);
                if(folder == null)
                {
                    CreateFolderArg args = new CreateFolderArg(fullPath, false);
                    var res = await this.Client.Files.CreateFolderV2Async(args);
                    folderId = res.Metadata.Id;
                }
                else
                {
                    folderId = folder.AsFolder.Id;
                }
            }).Wait();
            return folderId;
        }

        private string JoinPaths(params string[] paths)
        {
            if (paths != null)
            {
                string fullPath = $"/{RootFolderName}";
                for (int i = 0; i < paths.Length; i++)
                {
                    if(paths[i].Contains("/"))
                    {
                        throw new Exception("DropboxService.JoinPaths: paths contains illegal char");
                    }
                    fullPath += $"/{paths[i]}";
                }
                return fullPath;
            }
            else
            {
                throw new Exception("DropboxService.JoinPaths: paths can't be null");
            }
        }

        private async Task<Metadata> GetFolder(string fullPath)
        {
            GetMetadataArg arg = new GetMetadataArg(fullPath);
            try
            {
                var match = await this.Client.Files.GetMetadataAsync(arg);
                if(!match.IsFolder)
                {
                    throw new ConflictException($"DropboxService.GetFolder: '{fullPath}' is not a folder");
                }
                else
                {
                    return match;
                }
            }
            catch (Exception ex)
            {
                if(ex is Dropbox.Api.ApiException<Dropbox.Api.Files.GetMetadataError>)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public class ConflictException : Exception
        {
            public ConflictException(string errMsg) : base(errMsg)
            {
                
            }
        }

        public string UploadFile(string filename, Stream s, params string[] parentFolderPaths)
        {
            string fileId = null;
            string fullPath = this.JoinPaths(parentFolderPaths) + $"/{filename}";
            Task.Run(async () => 
            {
                UploadArg arg = new UploadArg(fullPath, WriteMode.Overwrite.Instance, false, DateTime.Now, true);                
                var res = await this.Client.Files.UploadAsync(arg, s);
                fileId = res.AsFile.Id;
                var link = await this.Client.Sharing.ListSharedLinksAsync(fileId);
                if (link.Links.Count == 0)
                {
                    await this.Client.Sharing.CreateSharedLinkWithSettingsAsync(fileId);
                }
            }).Wait();
            return fileId;
        }

        public string GetSharedLink(params string[] paths)
        {
            string url = null;
            Exception ex = null;
            string fullPath = this.JoinPaths(paths);
            if(_CachedSharedLink.ContainsKey(fullPath))
            {
                url = _CachedSharedLink[fullPath];
                return url;
            }
            else
            {
                var query = new GetMetadataArg(fullPath);
                Task.Run(async () =>
                {
                    try
                    {
                        var file = await this.Client.Files.GetMetadataAsync(query);
                        if (file.IsFile)
                        {
                            var link = await this.Client.Sharing.ListSharedLinksAsync(fullPath);
                            if (link.Links.Count == 0)
                            {
                                var res = await this.Client.Sharing.CreateSharedLinkWithSettingsAsync(fullPath);
                                url = res.Url;
                            }
                            else
                            {
                                url = link.Links[0].Url;
                            }
                        }
                        else
                        {
                            throw new Exception($"'{fullPath}' is not a file");
                        }
                    }
                    catch (Exception innerEx)
                    {
                        if (ex is Dropbox.Api.ApiException<Dropbox.Api.Files.GetMetadataError>)
                        {
                            ex = new Exception($"'{fullPath}' not found", innerEx);
                        }
                        else
                        {
                            ex = innerEx;
                        }
                    }
                }).Wait();
                if (ex == null)
                {
                    url = Regex.Replace(url, @"[?]dl=0", "?raw=1");
                    _CachedSharedLink[fullPath] = url;
                    return url;
                }
                else
                {
                    throw ex;
                }
            }
        }

        public void DeleteFile(params string[] fullPaths)
        {
            string fullPath = this.JoinPaths(fullPaths);
            Task.Run(async () => 
            {
                await this.Client.Files.DeleteV2Async(fullPath);
            }).Wait();
        }

        public void DeleteAll()
        {
            List<Metadata> files = new List<Metadata>();
            Task.Run(async () =>
            {
                var resp = await this.Client.Files.ListFolderAsync($"/{RootFolderName}");
                files.AddRange(resp.Entries);
                while(resp.HasMore)
                {
                    resp = await this.Client.Files.ListFolderAsync(resp.Cursor);
                    files.AddRange(resp.Entries);
                }
                for(int i = 0; i < files.Count; i++)
                {
                    if (files[i].AsFile != null)
                    {
                        await this.Client.Files.DeleteV2Async(files[i].AsFile.PathLower);
                    }
                    else if(files[i].AsFolder != null)
                    {
                        await this.Client.Files.DeleteV2Async(files[i].AsFolder.PathLower);
                    }
                }
            }).Wait();            
        }

        public void Dispose()
        {
            this.Client?.Dispose();
        }
    }
}
