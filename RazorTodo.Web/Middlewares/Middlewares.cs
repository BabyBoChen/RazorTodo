using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace RazorTodo.Web.Middlewares
{
    public class Middleware
    {
        public static Func<HttpContext, Func<Task>, Task> RequestLengthFilter { get; } = async (ctx, next) =>
        {
            long bodyLength = 0;
            try
            {
                ctx.Request.Form.Files.ToList().ForEach(f => bodyLength += f.Length);
            }
            catch (Exception) { }
            if(bodyLength <= 15728640)  //15MB
            {
                await next.Invoke();
            }
            else
            {
                ctx.Response.StatusCode = 413;
                await ctx.Response.WriteAsync("exceeding request size limitation");
            }
        };
    }
}
