using Microsoft.EntityFrameworkCore;
using RazorTodo.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Abstraction.Services
{
    public interface IDbContext : IDisposable
    {
        DbSet<GovernmentCalendar> GovernmentCalendars { get; set; }
        DbSet<Todo> Todos { get; set; }
        List<Todo> GetTodosByMonth(DateTime date);
        int SaveChanges();
        void SetProxyEnabled(bool isEnabled);
        string GetUid(string prefix = null, string suffix = null);
    }
}
