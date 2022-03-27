using System;
using System.Collections.Generic;

#nullable disable

namespace RazorTodo.DAL
{
    public partial class GovernmentCalendar
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
