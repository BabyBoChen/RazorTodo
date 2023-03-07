using RazorTodo.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Abstraction.Services
{
    public interface ICalendarService : IDisposable
    {
        void ImportGovernmentCalendar(string filepath);
        List<GovernmentCalendarDate> GetCalendarByYearAndMonth(int y, int m);
    }
}
