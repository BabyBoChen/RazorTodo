using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.Abstraction.Models;
using RazorTodo.Abstraction.Services;
using System.Collections.Generic;
using System.Linq;

namespace RazorTodo.Web.Pages
{
    [Authorize]
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class AlbumApiModel : PageModel
    {
        private IRazorTodoService service;
        private ICloudDriveService drive;
        public Todo Todo { get; set; }

        public AlbumApiModel(IRazorTodoService service, ICloudDriveService drive)
        {
            this.service = service;
            this.drive = drive;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            IActionResult resp = null;

            string id = Request.Form["TodoId"];
            int todoId = 0;
            if (!int.TryParse(id, out todoId))
            {
                resp = NotFound();
            }
            if (resp == null)
            {
                this.Todo = service.GetTodoByTodoId(todoId);
                if (this.Todo == null)
                {
                    resp = NotFound();
                }
            }
            if (resp == null)
            {
                List<string> actions = new List<string>
                {
                    "Delete", "Rename",
                };
                string action = Request.Form["Action"];
                if (action.IndexOf(action) == 0)
                {
                    resp = this.Delete();
                }
                else if (action.IndexOf(action) == 1)
                {
                    //resp = this.Rename();
                    resp = BadRequest();
                }
                else
                {
                    resp = BadRequest();
                }
            }
            return resp;
        }

        public IActionResult Delete()
        {
            List<string> toDelete = new List<string>();
            var entries = this.drive.GetFilesInFolder($"{Todo.TodoId}");
            var fileIds = Request.Form["FileId"].ToArray();
            for(int i = 0; i < fileIds.Length; i++)
            {
                var f = (from e in entries
                         where e.FileId == fileIds[i]
                         select e).FirstOrDefault() as CloudDriveFile;
                if(f != null)
                {
                    this.drive.DeleteFile($"{this.Todo.TodoId}", f.Filename);
                }
            }
            return Redirect($"/Album?id={this.Todo.TodoId}&p={Request.Form["PageNumber"]}");
        }
    }
}
