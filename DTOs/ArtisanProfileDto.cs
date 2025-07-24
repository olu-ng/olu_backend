using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using OluBackendApp.Models;

namespace OluBackendApp.DTOs
{
    // Returned in all GETs
    public record ProfileDto(
        string UserId,
        string Email,
        string Role,
        string? FirstName,
        string? LastName,
        DateTime? DateOfBirth,
        string? Gender,
        string? StateOfOrigin,
        string? ZipCode,
        string? StateOfResidence,
        string? Country,
        string? Profession,
        int? YearsOfExperience,
        List<string>? ServicesOffered,
        string? AboutYou,
        List<string>? ProfessionTags,
        List<string>? PhoneNumbers,
        string? Address,
        string? ProfilePictureUrl
    );

    // For /POST
    public record ProfileCreateDto(
        IFormFile ProfilePicture,
        string FirstName,
        string LastName,
        DateTime DateOfBirth,
        Gender Gender,
        StateOfOrigin StateOfOrigin,
        string ZipCode,
        string StateOfResidence,
        Country Country,
        string Profession,
        int YearsOfExperience,
        List<string> ServicesOffered,
        string AboutYou,
        List<string> ProfessionTags,
        List<string> PhoneNumbers,
        string Address
    );

    // For /PUT (full replace)
    public record ProfileUpdateDto(
        IFormFile ProfilePicture,
        string FirstName,
        string LastName,
        DateTime DateOfBirth,
        Gender Gender,
        StateOfOrigin StateOfOrigin,
        string ZipCode,
        string StateOfResidence,
        Country Country,
        string Profession,
        int YearsOfExperience,
        List<string> ServicesOffered,
        string AboutYou,
        List<string> ProfessionTags,
        List<string> PhoneNumbers,
        string Address
    );

    // For /PATCH (partial)
    public record ProfilePatchDto(
        IFormFile? ProfilePicture,
        string? FirstName,
        string? LastName,
        DateTime? DateOfBirth,
        Gender? Gender,
        StateOfOrigin? StateOfOrigin,
        string? ZipCode,
        string? StateOfResidence,
        Country? Country,
        string? Profession,
        int? YearsOfExperience,
        List<string>? ServicesOffered,
        string? AboutYou,
        List<string>? ProfessionTags,
        List<string>? PhoneNumbers,
        string? Address
    );

    // Admin DTOs for managing both user and profile
    public record AdminCreateArtisanDto(
        string Email,
        string Password,
        string ConfirmPassword,
        IFormFile ProfilePicture,
        string FirstName,
        string LastName,
        DateTime DateOfBirth,
        Gender Gender,
        StateOfOrigin StateOfOrigin,
        string ZipCode,
        string StateOfResidence,
        Country Country,
        string Profession,
        int YearsOfExperience,
        List<string> ServicesOffered,
        string AboutYou,
        List<string> ProfessionTags,
        List<string> PhoneNumbers,
        string Address
    );

    public record AdminUpdateArtisanDto(
        string? Email,
        string? Password,
        string? ConfirmPassword,
        IFormFile? ProfilePicture,
        string? FirstName,
        string? LastName,
        DateTime? DateOfBirth,
        Gender? Gender,
        StateOfOrigin? StateOfOrigin,
        string? ZipCode,
        string? StateOfResidence,
        Country? Country,
        string? Profession,
        int? YearsOfExperience,
        List<string>? ServicesOffered,
        string? AboutYou,
        List<string>? ProfessionTags,
        List<string>? PhoneNumbers,
        string? Address
    );

    public record AdminPatchArtisanDto(
        string? Email,
        string? Password,
        string? ConfirmPassword,
        IFormFile? ProfilePicture,
        string? FirstName,
        string? LastName,
        DateTime? DateOfBirth,
        Gender? Gender,
        StateOfOrigin? StateOfOrigin,
        string? ZipCode,
        string? StateOfResidence,
        Country? Country,
        string? Profession,
        int? YearsOfExperience,
        List<string>? ServicesOffered,
        string? AboutYou,
        List<string>? ProfessionTags,
        List<string>? PhoneNumbers,
        string? Address
    );
}
