using System.ComponentModel.DataAnnotations;

namespace OluBackendApp.Models
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int MessageId { get; set; }
        [Required]
        public string Url { get; set; } = null!;
        [Required]
        public string MimeType { get; set; } = null!;
        public string? Title { get; set; }

        public Message Message { get; set; } = null!;
    }
}
