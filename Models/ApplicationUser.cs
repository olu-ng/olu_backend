using Microsoft.AspNetCore.Identity;
namespace OluBackendApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Device fingerprint of last verified device
        public string? LastDeviceFingerprint { get; set; }

        // Navigation
        public ArtisanProfile? ArtisanProfile { get; set; }
        public OfficeOwnerProfile? OfficeOwnerProfile { get; set; }

        //public AdminProfile? AdminProfile { get; set; }
        //public SuperAdminProfile? SuperAdminProfile { get; set; }
    }
}
