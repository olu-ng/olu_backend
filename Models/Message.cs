using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using OluBackendApp.DTOs;

namespace OluBackendApp.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChatId { get; set; }

        [Required]
        public string SenderId { get; set; } = null!;

        [Required, StringLength(2000)]
        public string Content { get; set; } = null!;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }

        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; }

        public Chat Chat { get; set; } = null!;
        public ApplicationUser Sender { get; set; } = null!;
        public ICollection<Reaction>? Reactions { get; set; }
        public ICollection<Attachment>? Attachments { get; set; }

        // ✅ This is the correct navigation for threads
        public ICollection<ChatThread> Replies { get; set; } = new List<ChatThread>();
    }
}
