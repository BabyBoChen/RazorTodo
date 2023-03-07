using RazorTodo.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Abstraction.Services
{
    public interface IRazorTodoService : IDisposable
    {
        IOrderedQueryable<Todo> QueryTodos();
        int SaveTodoRowStates(List<TodoRowState> todoRowStates);
        Todo GetTodoByTodoId(int todoId);
        long GetMaxLineOrder();
        int SaveTodo(Todo todo, bool moveToTop);
        void AddTodo(Todo todo);
        List<Todo> GetTodosByMonth(DateTime date);
        GovernmentCalendar GetGovDate(string y, string m, string d);
        List<Todo> GetTodosByDate(string y, string m, string d);
        void SetProxyEnabled(bool isEnabled);
    }
}
