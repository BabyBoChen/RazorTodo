using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Abstraction.Models
{
    public class CloudDriveEntry
    {
        public string FileId { get; set; }
        public string SharedLink { get; set; }
    }
    
    public class CloudDriveFile : CloudDriveEntry
    {
        public string Filename { get; set; }
    }

    public class CloudDriveFolder : CloudDriveEntry
    {
        public string FolderName { get; set; }
    }
}
