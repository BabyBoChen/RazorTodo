using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorTodo.Web.Pages
{
    [Authorize]
    public class DownloadModel : PageModel
    {
        private readonly IWebHostEnvironment _root;
        public DownloadModel(IWebHostEnvironment root)
        {
            this._root = root;
        }
        public IActionResult OnGet()
        {
            string filepath = Path.Combine(_root.ContentRootPath, "RazorTodo.db");
            byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);
            return File(fileBytes, "application/octet-stream", "RazorTodo.db");
        }
    }
}
