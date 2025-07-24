// OluBackendApp.DTOs.OfficeOwnerProfileDtos.cs

namespace OluBackendApp.DTOs
{
    public record OfficeOwnerProfileDto(
        string UserId,
        string Email,
        string Role,
        string? FirstName,
        string? LastName,
        string? ZipCode,
        string? StateOfResidence,
        string? Address
    );

    public record OfficeOwnerProfileUpdateDto(
        string FirstName,
        string LastName,
        string ZipCode,
        string StateOfResidence,
        string Address
    );

    public record OfficeOwnerProfilePatchDto(
        string? FirstName,
        string? LastName,
        string? ZipCode,
        string? StateOfResidence,
        string? Address
    );

    public record ChangeEmailDto(
        string NewEmail,
        string ConfirmEmail,
        string CurrentPassword
    );

    public record ChangePasswordDto(
        string CurrentPassword,
        string NewPassword,
        string ConfirmPassword
    );
}
