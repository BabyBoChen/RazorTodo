using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazorTodo.DAL;
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
    }
}
