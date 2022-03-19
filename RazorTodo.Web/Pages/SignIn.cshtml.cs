using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorTodo.Web.Pages
{
    public class SignInModel : PageModel
    {
        public string ErrorMsg { get; set; }
        public string ReturlUrl { get; set; }
        public IActionResult OnGet()
        {
            string returnUrl = Request.Query["ReturnUrl"];
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                this.ReturlUrl = "~/";
            }
            else
            {
                this.ReturlUrl = returnUrl;
            }
            return Page();
        }
        public async Task<IActionResult> OnPost()
        {
            string password = Request.Form["Password"];
            string returnUrl = Request.Form["ReturnUrl"];
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
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return Redirect("~/");
                }
            }
            else
            {
                this.ErrorMsg = "請輸入正確的通關密語！";
                return Page();
            }
        }
    }
}
