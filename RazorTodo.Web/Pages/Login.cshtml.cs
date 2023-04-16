using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.Web.Secrets;

namespace RazorTodo.Web.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class LoginModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                var res = new { isAuthed = true };
                var jres = new JsonResult(res);
                return jres;
            }
            else
            {
                var res = new { isAuthed = false };
                var jres = new JsonResult(res);
                return jres;
            }
        }

        public async Task<IActionResult> OnPost()
        {
            string password = Request.Form["Password"];
            if (password == DbPassword.Password)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "BabyBo"),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20),
                    IsPersistent = false,
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                var res = new { isAuthed = true };
                var jres = new JsonResult(res);
                return jres;
            }
            else
            {
                var res = new { isAuthed = false };
                var jres = new JsonResult(res);
                return jres;
            }
        }
    }

    public class PasswordJson
    {
        public string Password { get; set; }
    }
}
