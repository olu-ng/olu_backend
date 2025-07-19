using OluBackendApp.Models;
using System.ComponentModel.DataAnnotations;
namespace OluBackendApp.Models
{
    public class ArtisanProfile
    {
        //public int Id { get; set; }
        //public string UserId { get; set; } = default!;
        //public ApplicationUser User { get; set; } = default!;
        //public string? ProfilePictureUrl { get; set; }
        //public string? Address { get; set; }
        //public string? PhoneNumber { get; set; }
        //public string? State { get; set; }


       
            //[Key]
            public int Id { get; set; }
            public string UserId { get; set; } = default!;

            public ApplicationUser User { get; set; } = default!;

            public string? ProfilePictureUrl { get; set; }
            public string? Address { get; set; }
            public string? PhoneNumber { get; set; }
            public string? State { get; set; }
        



    }
}
