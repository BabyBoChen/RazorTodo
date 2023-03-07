using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.Abstraction.Models;
using RazorTodo.Abstraction.Services;
using RazorTodo.Service;

namespace RazorTodo.Web.Pages
{
    [Authorize]
    public class DateDetailModel : PageModel
    {
        public DateTime? TargetDate { get; set; } = null;
        public GovernmentCalendar GovDate { get; set; }
        public List<Todo> Todos { get; set; } = new List<Todo>();

        private IRazorTodoService service;

        public DateDetailModel(IRazorTodoService service)
        {
            this.service = service;
        }

        public void OnGet()
        {
            string y = Request.Query["y"];
            string m = Request.Query["m"];
            string d = Request.Query["d"];
            DateTime targetDate = DateTime.MinValue;
            bool canParseTargetDate = DateTime.TryParse($"{y}-{m}-{d}", out targetDate);
            if (canParseTargetDate) 
            {
                this.TargetDate = targetDate;
            }
            this.GovDate = service.GetGovDate(y, m, d);
            this.Todos = service.GetTodosByDate(y, m, d);
            return;
        }
    }
}
