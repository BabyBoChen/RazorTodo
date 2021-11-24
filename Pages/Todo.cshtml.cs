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
using Microsoft.AspNetCore.Authorization;

namespace RazorTodo.Pages
{
    [Authorize]
    public class TodoModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _root;
        public string Id { get; set; }
        public Todo Todo { get; set; }
        public Mode Mode {get; set;}
        public AlertType AlertType  = AlertType.NoAlert;
        
        public TodoModel(ILogger<IndexModel> logger, IWebHostEnvironment root)
        {
            _logger = logger;
            _root = root;
        }

        public IActionResult OnGet()
        {
            string connString = Path.Combine(_root.ContentRootPath,"Db","RazorTodo.db");
            string mode = Request.Query["m"];
            string id = Request.Query["id"];
            int todoId = -1;

            string hasAlertStr = Request.Query["a"];
            if(hasAlertStr == "1")
            {
                this.AlertType = AlertType.Success;
            }
            else if (hasAlertStr == "2")
            {
                this.AlertType = AlertType.Fail;
            }

            if(int.TryParse(id, out todoId) && mode == "e")
            {
                this.Mode = Mode.Edit;
                var conn = new SQLiteConnection(connString);
                using(conn)
                {
                    this.Todo = conn.Query<Todo>("SELECT * FROM todo WHERE TodoId = ?;", todoId).FirstOrDefault();
                    conn.Close();
                }

                if(this.Todo != null)
                {
                    return Page();
                }
                else
                {
                    return NotFound();
                }
            }
            else if(mode == "a")
            {
                this.Mode = Mode.Add;
                var conn = new SQLiteConnection(connString);
                using(conn)
                {
                    this.Todo = new Todo();
                    conn.Close();
                }
                return Page();
            }
            else
            {
                return RedirectToPage("Index");
            }

        }
    
        public IActionResult OnPost(Todo todo)
        {
            if(!Request.Form.Keys.Contains("Mode") || (Request.Form["Mode"] != "Edit" && Request.Form["Mode"] != "Add"))
            {
                return Content("未指定存取模式");
            }

            if(Request.Form["Mode"] == "Edit")
            {
                string connString = Path.Combine(_root.ContentRootPath,"Db","RazorTodo.db");
                var conn = new SQLiteConnection(connString);
                try
                {
                    using(conn)
                    {
                        int lineOrder = 0;
                        if(Request.Form["MoveToTop"] == "1")
                        {
                            var max = conn.Query<MaxLineOrder>("SELECT MAX(LineOrder) as LineOrder FROM todo").FirstOrDefault();
                            if(max != null)
                            {
                                lineOrder = max.LineOrder.GetValueOrDefault();
                            }
                        }
                        DateTime createdDate = new DateTime();
                        bool createdDateCanParse = DateTime.TryParse(todo.CreatedDate, out createdDate);
                        if(createdDateCanParse)
                        {
                            todo.CreatedDate = createdDate.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            todo.CreatedDate = "NULL";
                        }
                        string sql = "";
                        if(Request.Form["MoveToTop"] == "1")
                        {
                            sql = "UPDATE todo SET TodoName = ?, IsDone = ?, CreatedDate = ?, Description = ?, LineOrder = ? WHERE TodoId = ?;";
                            conn.Execute(sql, todo.TodoName, todo.IsDone, todo.CreatedDate, todo.Description, lineOrder+1, todo.TodoId);
                        }
                        else
                        {
                            sql = "UPDATE todo SET TodoName = ?, IsDone = ?, CreatedDate = ?, Description = ? WHERE TodoId = ?;";
                            conn.Execute(sql, todo.TodoName, todo.IsDone, todo.CreatedDate, todo.Description, todo.TodoId);
                        }
                        conn.Close();
                        
                    }
                    return Redirect($"~/Todo?id={todo.TodoId}&m=e&a=1");
                }
                catch(Exception ex)
                {
                    return Content(ex.Message);
                }
                
            }
            else if (Request.Form["Mode"] == "Add")
            {
                string connString = Path.Combine(_root.ContentRootPath,"Db","RazorTodo.db");
                var conn = new SQLiteConnection(connString);
                try
                {
                    using(conn)
                    {
                        int lineOrder = 0;
                        var max = conn.Query<MaxLineOrder>("SELECT MAX(LineOrder) as LineOrder FROM todo").FirstOrDefault();
                        if(max != null)
                        {
                            lineOrder = max.LineOrder.GetValueOrDefault();
                        }
                        string sql = "";
                        if(string.IsNullOrWhiteSpace(todo.CreatedDate))
                        {
                            sql = "INSERT INTO todo(TodoName, IsDone, Description, LineOrder) VALUES(?,?,?,?);";
                            conn.Execute(sql, todo.TodoName, todo.IsDone, todo.Description, lineOrder+1 );
                        }
                        else
                        {
                            DateTime createdDate = new DateTime();
                            bool createdDateCanParse = DateTime.TryParse(todo.CreatedDate, out createdDate);
                            if(createdDateCanParse)
                            {
                                todo.CreatedDate = createdDate.ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                todo.CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                            }
                            sql = "INSERT INTO todo(TodoName, IsDone, CreatedDate, Description, LineOrder) VALUES(?,?,?,?,?);";
                            conn.Execute(sql, todo.TodoName, todo.IsDone, todo.CreatedDate, todo.Description, lineOrder+1 );
                        }
                        conn.Close();
                    }
                    return Redirect("~/Index?a=1");
                }
                catch(Exception ex)
                {
                    return Content(ex.Message);
                }
            }

            return BadRequest("Something went wrong...");
        }

    }

    public enum Mode
    {
        Add, Edit
    }

    public class MaxLineOrder
    {
        public int? LineOrder { get; set; }
    }

}
