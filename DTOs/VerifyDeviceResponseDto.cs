namespace OluBackendApp.DTOs
{
    // Dtos/VerifyDeviceResponseDto.cs
    public class VerifyDeviceResponseDto
    {
        public bool Verified { get; set; }
        public string Message { get; set; } = default!;
        public string NextStep { get; set; } = "login";
    }
}
