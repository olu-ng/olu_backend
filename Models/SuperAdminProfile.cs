// Models/SuperAdminProfile.cs
namespace OluBackendApp.Models
{
    public class SuperAdminProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
        // Add any superadmin‑specific profile fields here
    }
}