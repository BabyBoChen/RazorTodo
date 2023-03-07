using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.Abstraction.Models;
using RazorTodo.Abstraction.Services;

namespace RazorTodo.Web.Pages
{
    [Authorize]
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class TodoDetailModel : PageModel
    {
        private IRazorTodoService service;
        public string Id { get; set; }
        public Todo Todo { get; set; }
        public int ReturnPage { get; set; }
        public Mode Mode { get; set; }
        public AlertType AlertType = AlertType.NoAlert;

        public TodoDetailModel(IRazorTodoService service)
        {
            this.service = service;
        }

        public IActionResult OnGet()
        {
            string mode = Request.Query["m"];
            string id = Request.Query["id"];
            string p = Request.Query["p"];
            int returnPage = 0;
            if(int.TryParse(p, out returnPage))
            {
                this.ReturnPage = returnPage;
            }
            else
            {
                this.ReturnPage = 1;
            }


            string hasAlertStr = HttpContext.Session.GetString("AlertType");
            if (hasAlertStr == "1")
            {
                this.AlertType = AlertType.Success;
            }
            else if (hasAlertStr == "2")
            {
                this.AlertType = AlertType.Fail;
            }
            HttpContext.Session.SetString("AlertType", "");

            int todoId = 0;
            if (int.TryParse(id, out todoId) && mode == "e")
            {
                this.Mode = Mode.Edit;
                this.Todo = service.GetTodoByTodoId(todoId);
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
                    bool moveToTop = false;
                    if (Request.Form["MoveToTop"] == "1")
                    {
                        moveToTop = true;
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

                    this.service.SaveTodo(todo, moveToTop);

                    HttpContext.Session.SetString("AlertType", "1");

                    string p = Request.Form["ReturnPage"];
                    int returnPage = 0;
                    if (int.TryParse(p, out returnPage))
                    {
                        this.ReturnPage = returnPage;
                    }
                    else
                    {
                        this.ReturnPage = 1;
                    }

                    return Redirect($"~/TodoDetail?id={todo.TodoId}&m=e&p={this.ReturnPage}");
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
                    var newRow = new Todo();
                    newRow.TodoName = todo.TodoName;
                    newRow.IsDone = todo.IsDone;
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
                    newRow.CreatedDate = todo.CreatedDate;
                    newRow.Description = todo.Description;
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
                    newRow.EstDate = todo.EstDate;

                    service.AddTodo(newRow);

                    HttpContext.Session.SetString("AlertType", "1");

                    string p = Request.Form["ReturnPage"];
                    int returnPage = 0;
                    if (int.TryParse(p, out returnPage))
                    {
                        this.ReturnPage = returnPage;
                    }
                    else
                    {
                        this.ReturnPage = 1;
                    }

                    return Redirect($"~/#!/{this.ReturnPage}");
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
