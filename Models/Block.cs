using System.ComponentModel.DataAnnotations.Schema;

namespace OluBackendApp.Models
{
    public class Block
    {
        public int Id { get; set; }

        public string BlockerId { get; set; } = default!;
        public string BlockedId { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(BlockerId))]
        public ApplicationUser Blocker { get; set; } = null!;

        [ForeignKey(nameof(BlockedId))]
        public ApplicationUser Blocked { get; set; } = null!;
    }
}
