// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;

// namespace OluBackendApp.Models
// {
//     public class ChatThread
//     {
//         [Key]
//         public int Id { get; set; }
//         [Required]
//         public int ParentMsgId { get; set; }

//         public Message ParentMessage { get; set; } = null!;
//         public ICollection<Message> Replies { get; set; } = new List<Message>();

       
//     }
// }

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OluBackendApp.Models
{
    public class ChatThread
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ParentMsgId { get; set; }

        public Message ParentMessage { get; set; } = null!;
        public ICollection<Message> Replies { get; set; } = new List<Message>();

       
    }
}
