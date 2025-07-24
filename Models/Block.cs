namespace OluBackendApp.Models
{
    public class Block
    {
        public int Id { get; set; }
        public string BlockerId { get; set; } = default!;
        public string BlockedId { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

     

        public ApplicationUser Blocker { get; set; } = null!;

        public ApplicationUser Blocked { get; set; } = null!;

       
    }
}


