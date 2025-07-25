using OluBackendApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System;
using System.Collections.Generic;


namespace OluBackendApp.Models
{
    public class ArtisanProfile
    {
        [Key, ForeignKey(nameof(User))]
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        // Personal
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public Country? Country { get; set; }
        public State? StateOfOrigin { get; set; }
        public string? ZipCode { get; set; }
        

        // Professional
        public string? Profession { get; set; }
        public int? YearsOfExperience { get; set; }
        public List<string>? ServicesOffered { get; set; }
        public string? AboutYou { get; set; }
        public List<string>? ProfessionTags { get; set; }

        // Contact
        public List<string>? PhoneNumbers { get; set; }
        public string? Address { get; set; }

        // Avatar
        public string? ProfilePictureUrl { get; set; }

        // Soft‑delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
