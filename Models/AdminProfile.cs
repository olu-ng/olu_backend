// Models/AdminProfile.cs
namespace OluBackendApp.Models
{
    public class AdminProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
        // Add any admin‑specific profile fields here
    }
}