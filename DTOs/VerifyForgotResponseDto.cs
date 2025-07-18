namespace OluBackendApp.DTOs
{
    // Dtos/VerifyForgotResponseDto.cs
    public class VerifyForgotResponseDto
    {
        public bool Verified { get; set; }
        public string ResetToken { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string NextStep { get; set; } = "reset-password";
    }
}
