using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.DAL;

namespace RazorTodo.Web.Pages
{
    [Authorize]
    public class TodoDetailModel : PageModel
    {
        private RazorTodoContext db;
        public string Id { get; set; }
        public Todo Todo { get; set; }
        public Mode Mode { get; set; }
        public AlertType AlertType = AlertType.NoAlert;

        public TodoDetailModel(RazorTodoContext db)
        {
            this.db = db;
        }

        public IActionResult OnGet()
        {
            string mode = Request.Query["m"];
            string id = Request.Query["id"];
            int todoId = -1;

            string hasAlertStr = Request.Query["a"];
            if (hasAlertStr == "1")
            {
                this.AlertType = AlertType.Success;
            }
            else if (hasAlertStr == "2")
            {
                this.AlertType = AlertType.Fail;
            }

            if (int.TryParse(id, out todoId) && mode == "e")
            {
                this.Mode = Mode.Edit;
                this.Todo = (from t in this.db.Todos
                             where t.TodoId == todoId
                             select t).FirstOrDefault();
                if (this.Todo != null)
                {
                    return Page();
                }
                else
                {
                    return NotFound();
                }
            }
            else if (mode == "a")
            {
                this.Mode = Mode.Add;
                this.Todo = new Todo();
                return Page();
            }
            else
            {
                return RedirectToPage("Index");
            }

        }

        public IActionResult OnPost(Todo todo)
        {
            if (!Request.Form.Keys.Contains("Mode") || (Request.Form["Mode"] != "Edit" && Request.Form["Mode"] != "Add"))
            {
                return Content("未指定存取模式");
            }
            if (Request.Form["Mode"] == "Edit")
            {
                try
                {
                    long lineOrder = 0;
                    if (Request.Form["MoveToTop"] == "1")
                    {
                        lineOrder = this.db.Todos.Max(todo => todo.LineOrder).GetValueOrDefault();
                    }

                    DateTime createdDate = new DateTime();
                    bool createdDateCanParse = DateTime.TryParse(todo.CreatedDate, out createdDate);
                    if (createdDateCanParse)
                    {
                        todo.CreatedDate = createdDate.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        todo.CreatedDate = null;
                    }

                    DateTime estDate = new DateTime();
                    bool estDateCanParse = DateTime.TryParse(todo.EstDate, out estDate);
                    if (estDateCanParse)
                    {
                        todo.EstDate = estDate.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        todo.EstDate = null;
                    }

                    var row = (from t in this.db.Todos
                               where t.TodoId == todo.TodoId
                               select t).FirstOrDefault();
                    if(row != null)
                    {
                        row.TodoName = todo.TodoName;
                        row.IsDone = todo.IsDone;
                        row.Description = todo.Description;
                        row.CreatedDate = todo.CreatedDate;
                        if(Request.Form["MoveToTop"] == "1")
                        {
                            row.LineOrder = lineOrder + 1;
                        }
                        row.EstDate = todo.EstDate;
                        this.db.SaveChanges();
                    }
                    return Redirect($"~/TodoDetail?id={todo.TodoId}&m=e&a=1");
                }
                catch (Exception ex)
                {
                    return Content(ex.Message);
                }

            }
            else if (Request.Form["Mode"] == "Add")
            {
                try
                {
                    long lineOrder = 0;
                    lineOrder = this.db.Todos.Max(todo => todo.LineOrder).GetValueOrDefault();
                    var row = new Todo();
                    row.TodoName = todo.TodoName;
                    row.IsDone = todo.IsDone;
                    DateTime createdDate = new DateTime();
                    bool createdDateCanParse = DateTime.TryParse(todo.CreatedDate, out createdDate);
                    if (createdDateCanParse)
                    {
                        todo.CreatedDate = createdDate.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        todo.CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    }
                    row.CreatedDate = todo.CreatedDate;
                    row.Description = todo.Description;
                    row.LineOrder = lineOrder+1;
                    DateTime estDate = new DateTime();
                    bool estDateCanParse = DateTime.TryParse(todo.EstDate, out estDate);
                    if (estDateCanParse)
                    {
                        todo.EstDate = estDate.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        todo.EstDate = null;
                    }
                    row.EstDate = todo.EstDate;
                    this.db.Todos.Add(row);
                    this.db.SaveChanges();
                    return Redirect("~/Index?a=1");
                }
                catch (Exception ex)
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
}
