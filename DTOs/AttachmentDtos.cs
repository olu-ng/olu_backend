using System.ComponentModel.DataAnnotations;

namespace OluBackendApp.DTOs
{
    public record AttachmentDto(
        int Id,
        string Url,
        string MimeType,
        string? Title
    );
}
