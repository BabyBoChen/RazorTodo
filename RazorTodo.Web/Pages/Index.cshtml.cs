using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTodo.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using RazorTodo.Abstraction.Models;
using RazorTodo.Abstraction.Services;

namespace RazorTodo.Web.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class IndexModel : PageModel
    {
        private IRazorTodoService service;
        public List<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
        public AlertType AlertType = AlertType.NoAlert;

        public IndexModel(IRazorTodoService service)
        {
            this.service = service;
        }
        public void OnGet()
        {
            var todos = this.service.QueryTodos().Take(10);
            foreach(var todo in todos)
            {
                TodoItems.Add(new TodoItem(todo));
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
            HttpContext.Session.SetString("AlertType","");
        }

        public IActionResult OnPost()
        {
            string pageNumber = Request.Form["PageNumber"];
            if (!User.Identity.IsAuthenticated)
            {
                HttpContext.Session.SetString("AlertType", "2");
                return Redirect($"~/#!/{pageNumber}");
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

            try
            {
                service.SaveTodoRowStates(todoRowStates);
                HttpContext.Session.SetString("AlertType","1");
                return Redirect($"~/#!/{pageNumber}");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

        }
        
    }


    public enum AlertType
    {
        NoAlert = 0, Success = 1, Fail = 2
    }
}
