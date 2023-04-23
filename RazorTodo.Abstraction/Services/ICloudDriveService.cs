using RazorTodo.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Abstraction.Services
{
    public interface ICloudDriveService : IDisposable
    {
        List<CloudDriveEntry> GetFilesInFolder(params string[] folderPaths);
        string CreateFolder(string folderName, params string[] parentFolderPaths);
        string UploadFile(string filename, Stream s, params string[] parentFolderPaths);
        string GetSharedLink(params string[] paths);
        void DeleteFile(params string[] fullPaths);
        void DeleteAll();
    }
}
