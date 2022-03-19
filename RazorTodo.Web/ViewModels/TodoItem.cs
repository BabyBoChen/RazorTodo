using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazorTodo.DAL;

namespace RazorTodo.Web.ViewModels
{
    public class TodoItem : Todo
    {
        public TodoItem(Todo todo)
        {
            this.TodoId = todo.TodoId;
            this.TodoName = todo.TodoName;
            this.IsDone = todo.IsDone;
            this.CreatedDate = todo.CreatedDate;
            this.Description = todo.Description;
            this.LineOrder = todo.LineOrder;
            this.EstDate = todo.EstDate;
        }
        public int TabIndex { get; set; }
    }
}
