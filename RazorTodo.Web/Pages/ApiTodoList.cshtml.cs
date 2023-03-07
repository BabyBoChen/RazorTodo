using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.Abstraction.Services;

namespace RazorTodo.Web.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class ApiTodoListModel : ApiPageModel
    {
        private IRazorTodoService service;
        public ApiTodoListModel(IRazorTodoService service)
        {
            this.service = service;
        }
        public IActionResult OnGet(int? y, int? m)
        {
            DateTime? date = null;
            try
            {
                date = new DateTime(y.Value,m.Value,1);
            }
            catch (Exception)
            {
                date = DateTime.MinValue;
            }
            var todos = service.GetTodosByMonth(date.Value);
            service.SetProxyEnabled(false);
            var jres = new JsonResult(todos);
            return jres;
        }
        public IActionResult OnPost()
        {
            
            var jres = new JsonResult(new { First = 1, Last = "last" });
            return jres;
        }
    }
}
