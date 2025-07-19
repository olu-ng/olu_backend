// Models/AdminProfile.cs
using OluBackendApp.Models;
using System.ComponentModel.DataAnnotations;



// Models/AdminProfile.cs
namespace OluBackendApp.Models { 
public class AdminProfile
{
    //[Key] // Explicit for clarity, though EF infers it
    public int Id { get; set; }
    public string UserId { get; set; } = default!;

    public ApplicationUser User { get; set; } = default!;

    // Add any admin-specific fields here
}
}