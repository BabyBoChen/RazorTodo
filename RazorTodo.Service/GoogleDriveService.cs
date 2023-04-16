using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
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

        public string CreateFolder(string folderName)
        {
            var query = this._Service.Files.List();
            query.Q = "name = 'RazorTodo'";
            string rootId = query.Execute().Files.FirstOrDefault()?.Id;
            // folder infe
            var folderInfo = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { rootId },
            };
            var req = this._Service.Files.Create(folderInfo);
            req.Fields = "id"; 
            var file = req.Execute();
            return file?.Id;
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
