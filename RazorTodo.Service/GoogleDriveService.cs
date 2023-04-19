using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using RazorTodo.Abstraction.Models;
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
    public class GoogleDriveService : IDisposable
    {
        private DriveService _Service { get; set; }
        private string _ClientEmail { get; set; }
        public string RootFolderName { get; set; }

        public GoogleDriveService(string tokenPath, string rootFolderName)
        {
            string tokenJson = System.IO.File.ReadAllText(tokenPath);
            var token = JsonConvert.DeserializeObject<GoogleDriveApiToken>(tokenJson);
            this._ClientEmail = token.ClientEmail;
            using (var stream = new FileStream(tokenPath, FileMode.Open, FileAccess.Read))
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
            this.RootFolderName = rootFolderName;
        }

        public class GoogleDriveApiToken 
        {
            public string ClientEmail { get; set; }
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

        public string Upload(string filename, FileStream fs, params string[] parentFolderPaths)
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
                var request = this._Service.Files.Create(fileMetadata, fs, contentType);
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
                var request = this._Service.Files.Update(fileMetadata, file.Id, fs, contentType);
                request.Fields = "*";
                request.Upload();
                fileId = request.ResponseBody?.Id;
            }
            return fileId;
        }

        private Google.Apis.Drive.v3.Data.File GetFileFromFolder(string filename, Google.Apis.Drive.v3.Data.File parentFolder)
        {
            var req = this._Service.Files.List();
            req.Q = $"'{parentFolder.Id}' in parents and name = '{filename}'";
            var file = req.Execute().Files.FirstOrDefault();
            return file;
        }

        public string GetSharedLink(params string[] paths)
        {
            if(paths != null && paths.Length > 0)
            {
                var parentFolder = this.GetFolder(paths.Take(paths.Length - 1).ToArray());
                if (parentFolder != null)
                {
                    var file = this.GetFileFromFolder(paths.Last(), parentFolder);
                    if (file != null)
                    {
                        return this.GetSharedLinkById(file.Id);
                    }
                    else
                    {
                        throw new FileNotFoundException(this.JoinPaths(paths.Take(paths.Length - 1).ToArray()) + $"/{paths.Last()}");
                    }
                }
                else
                {
                    throw new FolderNotFoundException(this.JoinPaths(paths.Take(paths.Length - 1).ToArray()));
                }
            }
            else
            {
                throw new Exception("GoogleDriveService.GetSharedLink: paths can't be null or empty");
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

        private IList<Google.Apis.Drive.v3.Data.File> ListAllFiles(params string[] paths)
        {
            var req = this._Service.Files.List();
            req.Q = $"name = '{RootFolderName}' and mimeType = 'application/vnd.google-apps.folder'";
            string rootId = req.Execute().Files.FirstOrDefault()?.Id;
            req = this._Service.Files.List();
            req.Q = $" '{rootId}' in parents ";
            if (paths != null && paths.Length > 0)
            {
                string folderId = rootId;
                for (int i = 0; i < paths.Length; i++)
                {
                    req.Q = $" '{folderId}' in parents and name = '{paths[i]}' ";
                    folderId = req.Execute().Files.FirstOrDefault()?.Id;
                }
                req.Q = $" '{folderId}' in parents ";
            }
            req.Fields = "*";
            var files = req.Execute().Files;
            return files;
        }

        public void Dispose()
        {
            this._Service.Dispose();
        }
    }
}
