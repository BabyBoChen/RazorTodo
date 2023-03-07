using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Abstraction.Models
{
    public class GovernmentCalendar
    {
        public long GovernmentCalendarId { get; set; }
        public string DateString { get; set; }
        public long Year { get; set; }
        public long Month { get; set; }
        public long Date { get; set; }
        public long Day { get; set; }
        public long IsHoliday { get; set; } = 0;
        public string Description { get; set; }
    }
}
