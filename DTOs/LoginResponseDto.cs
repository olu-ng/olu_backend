
namespace OluBackendApp.DTOs { 
public class LoginResponseDto
{
    public bool RequiresOtp { get; set; }
    public string? OtpPurpose { get; set; }
    public bool Authenticated { get; set; }
    public string? Token { get; set; }
    public string? TokenExpiresAt { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string Message { get; set; } = default!;
    public string NextStep { get; set; } = default!;
}
}