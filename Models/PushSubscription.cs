using System.ComponentModel.DataAnnotations;

namespace OluBackendApp.Models
{
    public class PushSubscription
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        public string Endpoint { get; set; } = null!; // JSON endpoint
        [Required]
        public string P256DHKey { get; set; } = null!;
    }
}
