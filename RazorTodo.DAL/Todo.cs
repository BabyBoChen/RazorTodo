using System;
using System.Collections.Generic;

#nullable disable

namespace RazorTodo.DAL
{
    public partial class Todo
    {
        public long TodoId { get; set; }
        public string TodoName { get; set; }
        public long IsDone { get; set; }
        public string CreatedDate { get; set; }
        public string Description { get; set; }
        public long? LineOrder { get; set; }
        public string EstDate { get; set; }
    }
}
