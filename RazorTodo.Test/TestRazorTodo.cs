using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazorTodo.DAL;
using RazorTodo.Service;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                //var todo = db.GetTodosByMonth(new System.DateTime(2023,10,1));
            }
        }

        [TestMethod]
        public void TestImportGovernmentCalendar()
        {
            CalendarService service = new CalendarService();
            using (service)
            {
                service.ImportGovernmentCalendar("2024.csv");
            }
        }

        [TestMethod]
        public void TestGetCalendarByYearAndMonth()
        {
            using (var service = new CalendarService())
            {
                //var dateInfos = service.GetCalendarByYearAndMonth(2023,10);
            }
        }

        [TestMethod]
        public void TestGoogleDriveService()
        {
            //string cwd = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            //string secret = Path.Combine(cwd, "Secrets", "bblj-firebase.json");
            //using (var service = new GoogleDriveService(secret, "RazorTodo"))
            //{
            //    //string link = service.GetSharedLink("83", "test", "test.jpeg");
            //}
        }

        [TestMethod]
        public void TestDropboxService()
        {
            //string cwd = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            //string refreshTokenPath = Path.Combine(cwd, "Secrets", "RazorTodoDropboxRefreshToken.json");
            //string filePath = Path.Combine(cwd, "Temp", "ttttt.jpeg");
            ////20230422_225100000_1.jpeg
            //using (var service = new DropboxService(refreshTokenPath, "RazorTodo"))
            //{
            //    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            //    {
            //        service.UploadFile("20230422_225100000_1.jpeg", fs, "84");
            //    }
            //    //var url = service.GetSharedLink("gggg", "yyyy", "test.jpeg");               
            //}
        }

        [TestMethod]
        public void TestGetUid()
        {
            using (var db = new RazorTodoContext())
            {
                //string uid = db.GetUid("p","s");
            }
        }
    }
}
