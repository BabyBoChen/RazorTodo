using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.DAL;
using RazorTodo.Web.ViewModels;


namespace RazorTodo.Web.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class IndexModel : PageModel
    {
        private RazorTodoContext db;
        public List<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
        public AlertType AlertType = AlertType.NoAlert;
        public IndexModel(RazorTodoContext db)
        {
            this.db = db;
        }
        public void OnGet()
        {
            var todos = this.db.Todos.OrderByDescending(todo => todo.LineOrder);
            foreach(var todo in todos)
            {
                TodoItems.Add(new TodoItem(todo));
            }
            string hasAlertStr = Request.Query["a"];
            if (hasAlertStr == "1")
            {
                this.AlertType = AlertType.Success;
            }
            else if (hasAlertStr == "2")
            {
                this.AlertType = AlertType.Fail;
            }
        }

        public IActionResult OnPost()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("~/?a=2");
            }

            string[] todoIds = Request.Form["TodoId"];
            string[] isDones = Request.Form["IsDone"];
            string[] rowStates = Request.Form["RowState"];
            if (todoIds.Length != isDones.Length
                || todoIds.Length != rowStates.Length
                || isDones.Length != rowStates.Length)
            {
                return Content("資料格式不正確！");
            }

            List<TodoRowState> todoRowStates = new List<TodoRowState>();
            for (int i = 0; i < todoIds.Length; i++)
            {
                var todoRowState = new TodoRowState();
                todoRowState.TodoId = todoIds[i];
                todoRowState.IsDone = isDones[i];
                todoRowState.RowState = rowStates[i];
                todoRowStates.Add(todoRowState);
            }

            int rows = 0;
            try
            {
                foreach(var todoRowState in todoRowStates)
                {
                    if(todoRowState.RowState == "16")
                    {
                        var todo = (from t in this.db.Todos
                                    where t.TodoId == long.Parse(todoRowState.TodoId)
                                    select t).FirstOrDefault();
                        if(todo != null)
                        {
                            todo.IsDone = int.Parse(todoRowState.IsDone);
                        }
                    }
                    else if(todoRowState.RowState == "8")
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
                return Redirect("~/?a=1");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

        }
        
    }

    public class TodoRowState
    {
        public string TodoId { get; set; }
        public string IsDone { get; set; }
        public string RowState { get; set; }
    }

    public enum AlertType
    {
        NoAlert = 0, Success = 1, Fail = 2
    }
}
