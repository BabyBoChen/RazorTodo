using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Abstraction.Services
{
    public interface IImageService
    {
        FileInfo ResizeImage(Stream s, string fileName, int maxDimension);
    }
}
