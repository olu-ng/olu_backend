using System;
using System.ComponentModel.DataAnnotations;

namespace OluBackendApp.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Entity { get; set; } = null!;  // e.g. "Message"
        [Required]
        public string Action { get; set; } = null!;  // e.g. "Create"
        [Required]
        public string UserId { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }
        // added CreatedAt so controller logging works
        public DateTime CreatedAt { get; set; }

        public ApplicationUser User { get; set; } = null!;


    }
}
