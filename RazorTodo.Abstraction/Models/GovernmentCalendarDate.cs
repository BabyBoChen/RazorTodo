using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Abstraction.Models
{
    public class GovernmentCalendarDate
    {
        public string DateString { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Date { get; set; }
        public int Day { get; set; }
        public bool IsHoliday { get; set; }
        public string Description { get; set; }
    }
}
