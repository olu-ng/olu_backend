using System;
namespace OluBackendApp.DTOs
{
    /// <summary>
    /// Payload for POST /api/admin/officeowners
    /// </summary>
    public record AdminCreateOfficeOwnerDto(
        string Email,
        string Password,
        string ConfirmPassword,
        string FirstName,
        string LastName,
        string ZipCode,
        string StateOfResidence,
        string Address
    );

    /// <summary>
    /// Payload for PUT /api/admin/officeowners/{id}
    /// </summary>
    public record AdminUpdateOfficeOwnerDto(
        string? Email,
        string? Password,
        string? ConfirmPassword,
        string? FirstName,
        string? LastName,
        string? ZipCode,
        string? StateOfResidence,
        string? Address
    );

    /// <summary>
    /// Payload for PATCH /api/admin/officeowners/{id}
    /// </summary>
    public record AdminPatchOfficeOwnerDto(
        string? Email,
        string? Password,
        string? ConfirmPassword,
        string? FirstName,
        string? LastName,
        string? ZipCode,
        string? StateOfResidence,
        string? Address
    );
}
