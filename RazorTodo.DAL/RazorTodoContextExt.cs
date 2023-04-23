using Microsoft.EntityFrameworkCore;
using RazorTodo.Abstraction.Models;
using RazorTodo.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.DAL
{
    public partial class RazorTodoContext : DbContext, IDbContext
    {
        public List<Todo> GetTodosByMonth(DateTime date)
        {
            List<Todo> todos = new List<Todo>();
            this.Database.GetDbConnection().Open();
            var command = this.Database.GetDbConnection().CreateCommand();
            string date1 = (new DateTime(date.Year, date.Month, 1)).ToString("yyyy-MM-dd");
            string date2 = (new DateTime(date.Year, date.Month, 1).AddMonths(1)).ToString("yyyy-MM-dd");
            command.CommandText = $"SELECT * FROM todo WHERE EstDate >= '{date1}' AND EstDate < '{date2}'";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var todo = new Todo();
                todo.TodoId = reader.GetInt64(0);
                todo.TodoName = reader.GetString(1);
                todo.IsDone = reader.GetInt64(2);
                todo.CreatedDate = reader.GetString(3);
                todo.Description = reader.GetString(4);
                if (reader.GetValue(5) != DBNull.Value)
                {
                    todo.LineOrder = reader.GetInt64(5);
                }
                if (reader.GetValue(6) != DBNull.Value)
                {
                    todo.EstDate = reader.GetString(6);
                }
                todos.Add(todo);
            }
            return todos;
        }
        
        public string GetUid(string prefix = null, string suffix = null)
        {
            var db = new RazorTodoContext();
            var newUid = new TblUid();
            newUid.Prefix = prefix;
            newUid.Suffix = suffix;
            db.TblUids.Add(newUid);
            db.SaveChanges();
            db.Dispose();
            return $"{newUid.Prefix}{newUid.Uid}{newUid.Suffix}";
        }
    }
}
