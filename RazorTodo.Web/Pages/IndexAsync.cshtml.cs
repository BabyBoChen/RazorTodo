using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.DAL;
using RazorTodo.Web.ViewModels;
using RazorTodo.Service;
using Microsoft.AspNetCore.Authorization;

namespace RazorTodo.Web.Pages
{
    [Authorize]
    public class IndexAsyncModel : PageModel
    {
        private RazorTodoService service;
        public IndexAsyncModel(RazorTodoService service)
        {
            this.service = service;
        }
        public IActionResult OnGet(int? p)
        {
            List<TodoItem> items = new List<TodoItem>();
            
            if (p == null || p <= 0)
            {
                p = 1;
            }

            var todos = this.service.QueryTodos().Take(p.Value*10);
            int tabIndex = 0;
            foreach(var todo in todos)
            {
                tabIndex++;
                var item = new TodoItem(todo);
                item.TabIndex = tabIndex;
                items.Add(item);
            }
            return Partial("TodoItemList", items);
        }
    }
}
