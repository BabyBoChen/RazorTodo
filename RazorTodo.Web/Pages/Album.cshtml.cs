using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.Abstraction.Models;
using RazorTodo.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RazorTodo.Web.Pages
{
    [Authorize]
    public class AlbumModel : PageModel
    {
        private IRazorTodoService service;
        private ICloudDriveService drive;
        private IImageService imgService;
        public string Id { get; set; }
        public Todo Todo { get; set; }
        public int PageNumber { get; private set; } = 1;
        public int TotalPage { get; private set; }
        public List<CloudDriveFile> Photos { get; set; } = new List<CloudDriveFile>();

        public AlbumModel(IRazorTodoService service, IImageService imgService, ICloudDriveService drive)
        {
            this.service = service;
            this.imgService = imgService;
            this.drive = drive;
        }

        public IActionResult OnGet()
        {
            string id = Request.Query["id"];
            int todoId = 0;
            if (int.TryParse(id, out todoId))
            {
                this.Todo = service.GetTodoByTodoId(todoId);
                if (this.Todo != null)
                {
                    string p = Request.Query["p"];
                    int page = 1;
                    int.TryParse(p, out page);
                    var photoQuery = drive.GetFilesInFolder($"{todoId}");
                    var photos = new List<CloudDriveFile>();
                    for(int i = 0; i < photoQuery.Count; i++)
                    {
                        if (photoQuery[i] is CloudDriveFile)
                        {
                            photos.Add(photoQuery[i] as CloudDriveFile);
                        }
                    }
                    this.TotalPage = (int)Math.Ceiling(photos.Count / 8.0);
                    if(this.TotalPage == 0)
                    {
                        this.TotalPage = 1;
                    }
                    if(page <= 0)
                    {
                        page = 1;
                    }
                    else if(page > this.TotalPage)
                    {
                        page = this.TotalPage;
                    }
                    this.PageNumber = page;
                    this.Photos = photos.Skip((page - 1)*8).Take(8).ToList();
                    return Page();
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }            
        }
    
        public IActionResult OnPost()
        {
            IActionResult err = null;

            string id = Request.Form["TodoId"];
            int todoId = 0;
            if (!int.TryParse(id, out todoId))
            {
                err = NotFound();
            }
            if(err == null)
            {
                this.Todo = service.GetTodoByTodoId(todoId);
                if (this.Todo == null)
                {
                    err = NotFound();
                }
            }
            int photoCnt = Request.Form.Files.Count;
            long totalSize = 0;
            if(err == null)
            {
                var photos = Request.Form.Files;
                for (int i = 0; i < photos.Count; i++)
                {
                    totalSize += photos[i].Length;
                }
                if(photoCnt > 5)
                {
                    err = BadRequest("單次最多上傳5張照片!");
                }
                if(totalSize > 10485760)
                {
                    err = BadRequest("單次上傳照片不可超過10MB!");
                }
            }
            if(err == null)
            {
                var photos = Request.Form.Files;
                for (int i = 0; i < photos.Count; i++)
                {
                    FileInfo fi = null;
                    using (var s = photos[i].OpenReadStream())
                    {
                        fi = this.imgService.ResizeImage(s, photos[i].FileName, 800);
                    }
                    using (var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read))
                    {
                        this.drive.UploadFile(fi.Name, fs, $"{this.Todo.TodoId}");
                    }
                    try
                    {
                        fi.Directory?.Delete(true);
                    }
                    catch (Exception) { }
                }
            }
            if(err == null)
            {
                return Redirect($"/Album?id={this.Todo.TodoId}&p={Request.Form["PageNumber"]}");
            }
            else
            {
                return err;
            }
        }
    }
}
