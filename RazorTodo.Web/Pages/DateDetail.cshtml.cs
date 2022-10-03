using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.DAL;
using RazorTodo.Service;

namespace RazorTodo.Web.Pages
{
    public class DateDetailModel : PageModel
    {
        public DateTime? TargetDate { get; set; } = null;
        public GovernmentCalendar GovDate { get; set; }
        public List<Todo> Todos { get; set; } = new List<Todo>();


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
            using (var service = new RazorTodoService())
            {
                this.GovDate = service.GetGovDate(y, m, d);
                this.Todos = service.GetTodosByDate(y, m, d);
            }
            return;
        }
    }
}
