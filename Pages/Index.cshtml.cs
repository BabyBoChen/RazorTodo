using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using SQLite;
using System.Diagnostics;

namespace RazorTodo.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _root;
        public List<Todo> Todos = new List<Todo>();
        public AlertType AlertType = AlertType.NoAlert;
        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment root)
        {
            _logger = logger;
            _root = root;
        }

        public void OnGet()
        {
            string connString = Path.Combine(_root.ContentRootPath,"Db","RazorTodo.db");
            var conn = new SQLiteConnection(connString);
            using(conn)
            {
                this.Todos = conn.Query<Todo>("SELECT * FROM todo ORDER By LineOrder DESC, TodoId Desc;");
                conn.Close();
            }
            
            string hasAlertStr = Request.Query["a"];
            if(hasAlertStr == "1")
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
            if(todoIds.Length != isDones.Length 
                || todoIds.Length != rowStates.Length
                || isDones.Length != rowStates.Length)
            {
                return Content("資料格式不正確！");
            }

            List<TodoRowState> todoRowStates = new List<TodoRowState>();
            for(int i = 0; i < todoIds.Length; i++)
            {
                var todoRowState = new TodoRowState();
                todoRowState.TodoId = todoIds[i];
                todoRowState.IsDone = isDones[i];
                todoRowState.RowState = rowStates[i];
                todoRowStates.Add(todoRowState);
            }

            string connString = Path.Combine(_root.ContentRootPath,"Db","RazorTodo.db");
            var conn = new SQLiteConnection(connString);
            int rows = 0;
            try
            {
                using(conn)
                {
                    conn.BeginTransaction();
                    foreach(var todoRowState in todoRowStates)
                    {
                        if(todoRowState.RowState == "16")
                        {
                            rows += conn.Execute("UPDATE todo SET IsDone = ? WHERE TodoId = ?;",todoRowState.IsDone, todoRowState.TodoId );
                        }
                        else if (todoRowState.RowState == "8")
                        {
                            rows += conn.Execute("DELETE FROM todo WHERE TodoId = ?;", todoRowState.TodoId);
                        }
                    }
                    conn.Commit();
                    conn.Close();
                }
                return Redirect("~/?a=1");
            }
            catch(Exception ex)
            {
                return Content(ex.Message);
            }
            
        }

    }

    public class Todo
    {
        public int TodoId { get; set; }
        public string TodoName { get; set; }
        public int IsDone { get; set; }
        public string CreatedDate { get; set; }
        public string Description { get; set; }
        public int LineOrder { get; set; }
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
