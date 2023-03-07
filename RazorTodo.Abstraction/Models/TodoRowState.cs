using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Abstraction.Models
{
    public class TodoRowState
    {
        public string TodoId { get; set; }
        public string IsDone { get; set; }
        public string RowState { get; set; }
    }
}
