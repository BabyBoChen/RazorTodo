using System;

namespace RazorTodo.DTO
{
    public class TodoRowState
    {
        public string TodoId { get; set; }
        public string IsDone { get; set; }
        public string RowState { get; set; }
    }

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
