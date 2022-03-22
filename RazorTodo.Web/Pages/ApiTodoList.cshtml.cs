using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.Web.Filters;

namespace RazorTodo.Web.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class ApiTodoListModel : PageModel
    {
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Auth");
        }

        public IActionResult OnGet()
        {
            
            var jres = new JsonResult(new { First = 1, Last = "last" });
            return jres;
        }
        public IActionResult OnPost()
        {
            
            var jres = new JsonResult(new { First = 1, Last = "last" });
            return jres;
        }
    }
}
