using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using RazorTodo.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Service
{
    public class GoogleDriveService : IDisposable
    {
        private DriveService _Service { get; set; }
        private const string SERVICE_EMAIL = @"bblj-firebase-go@bblj-firebase.iam.gserviceaccount.com";

        public GoogleDriveService()
        {
            string cwd = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            string secret = Path.Combine(cwd, "Secrets", "bblj-firebase.json");
            using (var stream = new FileStream(secret, FileMode.Open, FileAccess.Read))
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
            req.Q = $"name = '{folderName}' and '{currentFolder.Id}' in parents and  mimeType = 'application/vnd.google-apps.folder'";
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
            req.Q = "name = 'RazorTodo' and mimeType = 'application/vnd.google-apps.folder'";
            Google.Apis.Drive.v3.Data.File currentFolder = req.Execute().Files.FirstOrDefault();  //root folder
            //string currentPath = "/";
            if (fullPaths == null || fullPaths.Length > 0)
            {
                req = this._Service.Files.List();
                for (int i = 0; i < fullPaths.Length; i++)
                {
                    //currentPath += fullPaths[i] + "/";
                    req.Q = $" '{currentFolder.Id}' in parents and name = '{fullPaths[i]}' and mimeType = 'application/vnd.google-apps.folder'";
                    currentFolder = req.Execute().Files.FirstOrDefault();
                }
            }
            return currentFolder;
        }

        //private class FolderNotFoundException : Exception
        //{
        //    public string FolderPath { get; private set; }
        //    public override string Message { get { return $"Folder \"{FolderPath}\" not found"; } }
        //    public FolderNotFoundException(string fullPath)
        //    {
        //        this.FolderPath = fullPath;
        //    }
        //}

        //public string Upload(string filename, FileStream fs, params string[] parentFolderPaths)
        //{
        //    string fileId = "";
        //    Google.Apis.Drive.v3.Data.File parentFolder = this.GetFolder(parentFolderPaths);
        //    if (parentFolder == null)
        //    {
        //        this.CreateFolder(parentFolderPaths.Last(), parentFolderPaths.Take(parentFolderPaths.Length - 1).ToArray());
        //        parentFolder = this.GetFolder(parentFolderPaths);
        //    }
        //    var file = this.GetFile(filename, parentFolderPaths);
        //    if(file == null)  //create & upload
        //    {
        //        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        //        {
        //            Name = filename,
        //            Parents = new List<string> { parentFolder.Id },
        //        };
        //        FilesResource.CreateMediaUpload request;
        //        request = this._Service.Files.Create(fileMetadata, fs, GoogleDriveContentType.GetContentTypeFromFileExtension((new FileInfo(filename)).Extension));
        //        request.Fields = "id";
        //        request.Upload();
                
        //    }
        //    else //update
        //    {

        //    }            
        //}

        private Google.Apis.Drive.v3.Data.File GetFile(string filename, params string[] parentFolderPaths)
        {
            var parent = this.GetFolder(parentFolderPaths);
            var req = this._Service.Files.List();
            req.Q = $"'{parent.Id}' in parents and name = '{filename}'";
            var file = req.Execute().Files.FirstOrDefault();
            return file;
        }

        public IList<Google.Apis.Drive.v3.Data.File> ListAllFiles(params string[] paths)
        {
            var req = this._Service.Files.List();
            req.Q = "name = 'RazorTodo'";
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

        public void DeleteAll()
        {
            var myFiles = this.ListAllFiles().Where((f) => 
            {
                bool isOwner = false;
                foreach(var user in f.Owners)
                {
                    if(user.EmailAddress == SERVICE_EMAIL)
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

        public void Dispose()
        {
            this._Service.Dispose();
        }
    }
}
