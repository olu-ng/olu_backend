
namespace OluBackendApp.DTOs { 

// Dtos/OtpVerifyResponseDto.cs
public class OtpVerifyResponseDto
{
    public bool Verified { get; set; }
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string NextStep { get; set; } = "login";
}
}