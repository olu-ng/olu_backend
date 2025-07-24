using System.ComponentModel.DataAnnotations;

namespace OluBackendApp.DTOs
{
    public record CreatePushSubscriptionDto(
        [Required] string Endpoint,
        [Required] string P256DHKey
    );
}
