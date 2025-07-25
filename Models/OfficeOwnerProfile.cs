using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OluBackendApp.Models
{
    public class OfficeOwnerProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = default!;

        public ApplicationUser User { get; set; } = default!;

        /// <summary>URL to the profile picture (max 2048 chars).</summary>
        [Url]
        [StringLength(2048)]
        public string? ProfilePictureUrl { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = default!;

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string StateOfResidence { get; set; } = default!;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = default!;

        /// <summary>Stored as a JSON array in the database (requires provider support).</summary>
        [Column(TypeName = "jsonb")]        // For PostgreSQL; remove or change for other DBs
        public List<string>? PhoneNumbers { get; set; }

        // Auditing & soft delete:
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Job posts created by this Office Owner
        public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
    }
}
