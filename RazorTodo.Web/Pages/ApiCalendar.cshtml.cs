using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.Service;

namespace RazorTodo.Web.Pages
{
    public class ApiCalendarModel : ApiPageModel
    {
        private CalendarService service;
        public ApiCalendarModel(CalendarService service)
        {
            this.service = service;
        }
        public IActionResult OnGet(int? y, int? m)
        {
            int year = y.GetValueOrDefault();
            int month = m.GetValueOrDefault();
            var calendars = this.service.GetCalendarByYearAndMonth(year, month);
            JsonResult jres = new JsonResult(calendars);
            return jres;
        }
    }
}
