using System;
using System.Linq;
using RazorTodo.DAL;
using System.Diagnostics;
using RazorTodo.DTO;
using System.Collections.Generic;

namespace RazorTodo.Service
{
    public class RazorTodoService : IDisposable
    {
        private RazorTodoContext db = new RazorTodoContext();

        public IOrderedQueryable<Todo> QueryTodos()
        {
            var todos = from t in this.db.Todos
                         orderby t.LineOrder descending
                         select t;
            return todos;
        }

        public int SaveTodoRowStates(List<TodoRowState> todoRowStates)
        {
            int rows = 0;
            foreach (var todoRowState in todoRowStates)
            {
                if (todoRowState.RowState == "16")
                {
                    var todo = (from t in this.db.Todos
                                where t.TodoId == long.Parse(todoRowState.TodoId)
                                select t).FirstOrDefault();
                    if (todo != null)
                    {
                        todo.IsDone = int.Parse(todoRowState.IsDone);
                    }
                }
                else if (todoRowState.RowState == "8")
                {
                    var todo = (from t in this.db.Todos
                                where t.TodoId == long.Parse(todoRowState.TodoId)
                                select t).FirstOrDefault();
                    if (todo != null)
                    {
                        this.db.Todos.Remove(todo);
                    }
                }
            }
            rows = this.db.SaveChanges();
            return rows;
        }

        public Todo GetTodoByTodoId(int todoId)
        {
            var todo = (from t in this.db.Todos
                        where t.TodoId == todoId
                        select t).FirstOrDefault();
            return todo;
        }

        public long GetMaxLineOrder()
        {
            long lineOrder = this.db.Todos.Max(todo => todo.LineOrder).GetValueOrDefault();
            return lineOrder;
        }

        public int SaveTodo(Todo todo, bool moveToTop)
        {
            int affectedRow = 0;
            var row = (from t in this.db.Todos
                       where t.TodoId == todo.TodoId
                       select t).FirstOrDefault();
            if (row != null)
            {
                row.TodoName = todo.TodoName;
                row.IsDone = todo.IsDone;
                row.Description = todo.Description;
                row.CreatedDate = todo.CreatedDate;
                if (moveToTop)
                {
                    row.LineOrder = this.GetMaxLineOrder() + 1;
                }
                row.EstDate = todo.EstDate;
                affectedRow = this.db.SaveChanges();
            }
            return affectedRow;
        }

        public void AddTodo(Todo todo)
        {
            todo.LineOrder = GetMaxLineOrder() + 1;
            this.db.Todos.Add(todo);
        }

        public List<Todo> GetTodosByMonth(DateTime date)
        {
            var todos = this.db.GetTodosByMonth(date);
            return todos;
        }

        public void TurnOffProxy()
        {
            this.db.ChangeTracker.LazyLoadingEnabled = false;
        }

        public void Dispose()
        {
            this.db?.Dispose();
            Debug.WriteLine("dispose");
        }

        ~RazorTodoService()
        {
            this.db?.Dispose();
            Debug.WriteLine("finalize");
        }

    }
}
