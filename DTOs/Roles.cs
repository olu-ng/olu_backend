
namespace OluBackendApp.DTOs
{
    public static class Roles
    {
        public const string Artisan = "Artisan";
        public const string OfficeOwner = "OfficeOwner";
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
    }

    public record RegisterDto(string Email, string Password, string ConfirmPassword, string Role);
    public record LoginDto(string Email, string Password);
    public record ForgotDto(string Email);
    public record OtpVerifyDto(string Email, string Code);
    public record ResetDto(string Email, string Code, string NewPassword);
    public record ResendOtpRequestDto(string Email, string WhyResentOtp); // Flow = "registration","forgot","new-device"
    public record ChangeDto(string CurrentPassword, string NewPassword);

  
        /// <summary>
        /// Payload for resetting a password: includes optional reset token.
        /// </summary>
        public record ResetPasswordDto(
            string Email,
            string NewPassword,
            string? ResetToken);

        /// <summary>
        /// Profile data returned for the current user.
        /// </summary>
        public class UserProfileDto
        {
            public string UserId { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Role { get; set; } = default!;
        }

    }
