using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazorTodo.DAL;
using RazorTodo.Service;
using System.IO;
using System.Linq;
using System.Reflection;

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
                var todo = db.GetTodosByMonth(new System.DateTime(2023,10,1));
            }
        }

        [TestMethod]
        public void TestImportGovernmentCalendar()
        {
            CalendarService service = new CalendarService();
            using (service)
            {
                //service.ImportGovernmentCalendar("2023.csv");
            }
        }

        [TestMethod]
        public void TestGetCalendarByYearAndMonth()
        {
            using (var service = new CalendarService())
            {
                var dateInfos = service.GetCalendarByYearAndMonth(2023,10);
            }
        }

        [TestMethod]
        public void TestGoogleDriveService()
        {
            string cwd = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            string secret = Path.Combine(cwd, "Secrets", "bblj-firebase.json");
            using (var service = new GoogleDriveService(secret, "RazorTodo"))
            {
                string link = service.GetSharedLink("83", "test", "test.jpeg");
            }
        }
    }
}
