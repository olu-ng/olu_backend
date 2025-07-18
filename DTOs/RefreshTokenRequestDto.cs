namespace OluBackendApp.DTOs
{
    // Dtos/RefreshTokenRequestDto.cs
    public record RefreshTokenRequestDto(string RefreshToken);

    // Dtos/RefreshTokenResponseDto.cs
    public class RefreshTokenResponseDto
    {
        public string Token { get; set; } = default!;
        public string ExpiresAt { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }

}
