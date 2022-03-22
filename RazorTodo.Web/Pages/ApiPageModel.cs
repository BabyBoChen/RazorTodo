using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorTodo.Web.Pages
{
    public class ApiPageModel : PageModel
    {
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "*");
            Response.Headers.Add("Access-Control-Allow-Headers", "Auth");
            if (context.HttpContext.Request.Method.ToUpper() == "OPTIONS")
            {
                context.Result = new OkResult();
            }
            else
            {
                return;
            }
        }
    }
}
