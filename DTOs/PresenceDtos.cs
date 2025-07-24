using System;
using OluBackendApp.Models;
using static OluBackendApp.Models.ApplicationUser;

namespace OluBackendApp.DTOs
{
    public record UserPresenceDto(
        string UserId,
        PresenceStatus Status,
        DateTime? LastSeenAt,
        string? StatusMessage
    );
}
