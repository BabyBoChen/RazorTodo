using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorTodo.Abstraction.Models
{
    public partial class Todo
    {
        public Todo()
        {
            Photos = new HashSet<Photo>();
        }

        public long TodoId { get; set; }
        public string TodoName { get; set; }
        public long IsDone { get; set; }
        public string CreatedDate { get; set; }
        public string Description { get; set; }
        public long? LineOrder { get; set; }
        public string EstDate { get; set; }

        public virtual ICollection<Photo> Photos { get; set; }
    }
}
