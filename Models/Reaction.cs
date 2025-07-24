using System.ComponentModel.DataAnnotations;

namespace OluBackendApp.Models
{
    public class Reaction
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int MessageId { get; set; }
        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        public string Emoji { get; set; } = null!; // e.g. "👍"

        public Message Message { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
