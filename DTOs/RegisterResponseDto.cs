


namespace OluBackendApp.DTOs { 

public class RegisterResponseDto
{
    public bool RequiresOtp { get; set; }
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string OtpPurpose { get; set; } = default!;
    public DateTime OtpSentAt { get; set; }
    public string NextStep { get; set; } = "Please verify the OTP sent to your email.";
}
}