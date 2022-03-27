using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazorTodo.DAL;
using RazorTodo.Service;
using System.Linq;

namespace RazorTodo.Test
{
    [TestClass]
    public class TestRazorTodo
    {
        [TestMethod]
        public void TestGetTodosByMonth()
        {
            var db = new RazorTodoContext();
            using (db)
            {
                var todo = db.GetTodosByMonth(new System.DateTime(2022,2,1));
            }
        }

        [TestMethod]
        public void TestImportGovernmentCalendar()
        {
            var service = new CalendarService();
            service.ImportGovernmentCalendar("2020.csv");
            service.ImportGovernmentCalendar("2021.csv");
            service.ImportGovernmentCalendar("2022.csv");
        }
    }
}
