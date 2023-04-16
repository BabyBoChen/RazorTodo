using System;
using System.Collections.Generic;

#nullable disable

namespace RazorTodo.Abstraction.Models
{
    public partial class Photo
    {
        public long PhotoId { get; set; }
        public long TodoId { get; set; }
        public string FileId { get; set; }
        public string Uid { get; set; }

        public virtual Todo Todo { get; set; }
    }
}
