namespace OluBackendApp.DTOs
{
    public class ResendOtpResponseDto
    {
        public bool RequiresOtp { get; set; }
        public string NextStep { get; set; } = default!;
    }
}
