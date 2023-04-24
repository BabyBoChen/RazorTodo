using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using RazorTodo.Abstraction.Models;
using RazorTodo.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Service
{
    public class GoogleDriveService : ICloudDriveService
    {
        private static string _TokenPath { get; set; }
        private static string _RootFolderName { get; set; }
        private static bool _IsRegister = false;
        public static void Register(string tokenPath, string rootFolderName)
        {
            _TokenPath = tokenPath;
            _RootFolderName = rootFolderName;
            _IsRegister = true;
        }
        
        private DriveService _Service { get; set; }
        private string _ClientEmail { get; set; }
        public string RootFolderName { get; private set; }
        private static Dictionary<string, string> _CachedSharedLink { get; } = new Dictionary<string, string>();

        public GoogleDriveService()
        {
            if(_IsRegister)
            {
                string tokenJson = System.IO.File.ReadAllText(_TokenPath);
                var token = JsonConvert.DeserializeObject<GoogleDriveApiToken>(tokenJson);
                this._ClientEmail = token.ClientEmail;
                using (var stream = new FileStream(_TokenPath, FileMode.Open, FileAccess.Read))
                {
                    var credentials = GoogleCredential.FromStream(stream);
                    if (credentials.IsCreateScopedRequired)
                    {
                        credentials = credentials.CreateScoped(new string[] { DriveService.Scope.Drive });
                    }

                    this._Service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credentials,
                        ApplicationName = "RazorTodo",
                    });
                }
                this.RootFolderName = _RootFolderName;
            }
            else
            {
                throw new Exception("This service has not been registered yet. Please invoke GoogleDriveService.Register before instantiating this service.");
            }
        }

        public class GoogleDriveApiToken 
        {
            public string ClientEmail { get; set; }
        }

        public List<CloudDriveEntry> GetFilesInFolder(params string[] folderPaths)
        {
            List<CloudDriveEntry> files = new List<CloudDriveEntry>();
            var folder = this.GetFolder(folderPaths);
            if(folder != null)
            {
                var metas = this.ListAllFiles(folderPaths);
                for(int i = 0; i < metas.Count; i++)
                {
                    var meta = metas[i];
                    if(meta.MimeType != "application/vnd.google-apps.folder")
                    {
                        var f = new CloudDriveFile();
                        f.FileId = meta.Id;
                        f.Filename = meta.Name;
                        f.SharedLink = this.GetSharedLinkById(f.FileId);
                        files.Add(f);
                    }
                    else if(meta.MimeType == "application/vnd.google-apps.folder")
                    {
                        var f = new CloudDriveFolder();
                        f.FileId = meta.Id;
                        f.FolderName = meta.Name;
                        f.SharedLink = null;
                        files.Add(f);
                    }
                }
            }
            return files;
        }

        public string CreateFolder(string folderName, params string[] parentFolderPaths)
        {
            var currentFolder = this.GetFolder();
            List<string> paths = new List<string>();
            for (int i = 0; i < parentFolderPaths.Length; i++)
            {
                paths.Add(parentFolderPaths[i]);
                currentFolder = this.GetFolder(paths.ToArray());
                if(currentFolder == null)
                {
                    this.CreateFolder(parentFolderPaths[i], paths.Take(i).ToArray());
                    currentFolder = this.GetFolder(paths.ToArray());
                }
            }
            var req = this._Service.Files.List();
            req.Q = $"name = '{folderName}' and '{currentFolder.Id}' in parents and mimeType = 'application/vnd.google-apps.folder'";
            var createdFolder = req.Execute().Files.FirstOrDefault();
            if(createdFolder == null)
            {
                // File metadata
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = new List<string> { currentFolder.Id },
                };
                // Create a new folder on drive.
                var request = this._Service.Files.Create(fileMetadata);
                request.Fields = "id";
                createdFolder = request.Execute();
            }
            return createdFolder?.Id;
        }

        private Google.Apis.Drive.v3.Data.File GetFolder(params string[] fullPaths)
        {
            var req = this._Service.Files.List();
            req.Q = $"name = '{RootFolderName}' and mimeType = 'application/vnd.google-apps.folder'";
            Google.Apis.Drive.v3.Data.File currentFolder = req.Execute().Files.FirstOrDefault();  //root folder
            //string currentPath = "/";
            if (fullPaths != null && fullPaths.Length > 0)
            {
                req = this._Service.Files.List();
                for (int i = 0; i < fullPaths.Length; i++)
                {
                    //currentPath += fullPaths[i] + "/";
                    req.Q = $" '{currentFolder.Id}' in parents and name = '{fullPaths[i]}' and mimeType = 'application/vnd.google-apps.folder'";
                    currentFolder = req.Execute().Files.FirstOrDefault();
                    if(currentFolder == null)
                    {
                        break;
                    }
                }
            }
            return currentFolder;
        }

        public string UploadFile(string filename, Stream s, params string[] parentFolderPaths)
        {
            string fileId = "";
            Google.Apis.Drive.v3.Data.File parentFolder = this.GetFolder(parentFolderPaths);
            if (parentFolder == null)
            {
                this.CreateFolder(parentFolderPaths.Last(), parentFolderPaths.Take(parentFolderPaths.Length - 1).ToArray());
                parentFolder = this.GetFolder(parentFolderPaths);
            }
            string contentType = GoogleDriveContentType.GetContentTypeFromFileExtension((new FileInfo(filename)).Extension);
            var file = this.GetFileFromFolder(filename, parentFolder);
            if (file == null)  //create & upload
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = filename,
                    Parents = new List<string> { parentFolder.Id },
                };
                var request = this._Service.Files.Create(fileMetadata, s, contentType);
                request.Fields = "*";
                request.Upload();
                fileId = request.ResponseBody?.Id;
            }
            else //update
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = filename,
                };
                var request = this._Service.Files.Update(fileMetadata, file.Id, s, contentType);
                request.Fields = "*";
                request.Upload();
                fileId = request.ResponseBody?.Id;
            }
            return fileId;
        }

        private Google.Apis.Drive.v3.Data.File GetFileFromFolder(string filename, Google.Apis.Drive.v3.Data.File parentFolder)
        {
            var req = this._Service.Files.List();
            req.Q = $"'{parentFolder.Id}' in parents and name = '{filename}' and mimeType != 'application/vnd.google-apps.folder'";
            var file = req.Execute().Files.FirstOrDefault();
            return file;
        }

        public string GetSharedLink(params string[] paths)
        {
            string sharedLink = null;
            Exception err = null;
            if (paths != null && paths.Length > 0)
            {
                string fullPath = this.JoinPaths(paths);
                if(_CachedSharedLink.ContainsKey(fullPath))
                {
                    sharedLink = _CachedSharedLink[fullPath];
                }
                else
                {
                    var parentFolder = this.GetFolder(paths.Take(paths.Length - 1).ToArray());
                    if (parentFolder != null)
                    {
                        var file = this.GetFileFromFolder(paths.Last(), parentFolder);
                        if (file != null)
                        {
                            _CachedSharedLink[fullPath] = this.GetSharedLinkById(file.Id);
                            sharedLink = _CachedSharedLink[fullPath];
                        }
                        else
                        {
                            err = new FileNotFoundException(this.JoinPaths(paths.Take(paths.Length - 1).ToArray()) + $"/{paths.Last()}");
                        }
                    }
                    else
                    {
                        err = new FolderNotFoundException(this.JoinPaths(paths.Take(paths.Length - 1).ToArray()));
                    }
                }
            }
            else
            {
                err = new Exception("GoogleDriveService.GetSharedLink: paths can't be null or empty");
            }
            if (err == null)
            {
                return sharedLink;
            }
            else
            {
                throw err;
            }
        }

        private string JoinPaths(params string[] paths)
        {
            if(paths != null)
            {
                string fullPath = $"/{this.RootFolderName}";
                for (int i = 0; i < paths.Length; i++)
                {
                    fullPath += $"/{paths[i]}";
                }
                return fullPath;
            }
            else
            {
                throw new Exception("GoogleDriveService.JoinPaths: paths can't be null");
            }
        }

        public class FileNotFoundException : Exception
        {
            public string FullPath { get; private set; }
            public override string Message { get { return $"File \"{FullPath}\" not found"; } }
            public FileNotFoundException(string fullPath)
            {
                this.FullPath = fullPath;
            }
        }

        public class FolderNotFoundException : Exception
        {
            public string FolderPath { get; private set; }
            public override string Message { get { return $"Folder \"{FolderPath}\" not found"; } }
            public FolderNotFoundException(string fullPath)
            {
                this.FolderPath = fullPath;
            }
        }

        public string GetSharedLinkById(string fileId)
        {
            string link = $@"https://drive.google.com/uc?export=view&id={fileId}";
            return link;
        }

        public void DeleteFile(params string[] fullPaths)
        {
            if (fullPaths != null && fullPaths.Length > 0)
            {
                var parentFolder = this.GetFolder(fullPaths.Take(fullPaths.Length - 1).ToArray());
                if (parentFolder != null)
                {
                    var file = this.GetFileFromFolder(fullPaths.Last(), parentFolder);
                    if (file != null)
                    {
                        this._Service.Files.Delete(file.Id);
                    }
                    else
                    {
                        throw new FileNotFoundException(this.JoinPaths(fullPaths.Take(fullPaths.Length - 1).ToArray()) + $"/{fullPaths.Last()}");
                    }
                }
                else
                {
                    throw new FolderNotFoundException(this.JoinPaths(fullPaths.Take(fullPaths.Length - 1).ToArray()));
                }
            }
        }

        public void DeleteAll()
        {
            var myFiles = this.ListAllFiles().Where((f) => 
            {
                bool isOwner = false;
                foreach(var user in f.Owners)
                {
                    if(user.EmailAddress == _ClientEmail)
                    {
                        isOwner = true;
                        break;
                    }
                }
                return isOwner;
            });
            foreach(var f in myFiles)
            {
                var req = this._Service.Files.Delete(f.Id).Execute();
            }
        }

        private IList<Google.Apis.Drive.v3.Data.File> ListAllFiles(params string[] folderPaths)
        {
            var req = this._Service.Files.List();
            req.Q = $"name = '{RootFolderName}' and mimeType = 'application/vnd.google-apps.folder'";
            string rootId = req.Execute().Files.FirstOrDefault()?.Id;
            req = this._Service.Files.List();
            req.Q = $" '{rootId}' in parents ";
            if (folderPaths != null && folderPaths.Length > 0)
            {
                string folderId = rootId;
                for (int i = 0; i < folderPaths.Length; i++)
                {
                    req.Q = $" '{folderId}' in parents and name = '{folderPaths[i]}' ";
                    folderId = req.Execute().Files.FirstOrDefault()?.Id;
                }
                req.Q = $" '{folderId}' in parents ";
            }
            req.Fields = "*";
            var files = req.Execute().Files;
            return files;
        }

        public void ClearCache()
        {
            _CachedSharedLink.Clear();
        }

        public void Dispose()
        {
            this._Service.Dispose();
        }
    }
}
