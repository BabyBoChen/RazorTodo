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
    public class AlbumModel : PageModel
    {
        private IRazorTodoService service;
        public string Id { get; set; }
        public Todo Todo { get; set; }
        public List<Photo> Photos { get; set; } = new List<Photo>();

        public AlbumModel(IRazorTodoService service)
        {
            this.service = service;
        }

        public IActionResult OnGet()
        {
            string id = Request.Query["id"];
            int todoId = 0;
            if (int.TryParse(id, out todoId))
            {
                this.Todo = service.GetTodoByTodoId(todoId);
                this.Photos = this.Todo.Photos.ToList();
                if (this.Todo != null)
                {
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
    }
}
