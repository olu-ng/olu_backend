using OluBackendApp.Models;
using System.ComponentModel.DataAnnotations;
namespace OluBackendApp.Models
{
    public enum OtpPurpose { Registration, ForgotPassword, ChangePassword, NewDevice }

    public class OtpRecord
    {
        public int Id { get; set; }
        //[Key]
        [Required] public string UserId { get; set; } = default!;
        [Required] public string Code { get; set; } = default!;
        [Required] public OtpPurpose Purpose { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public string? DeviceFingerprint { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
