namespace OluBackendApp.Models
{
    public class JobPost
    {
        public int Id { get; set; } // int, not Guid
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public int OfficeOwnerProfileId { get; set; }
        public OfficeOwnerProfile OfficeOwnerProfile { get; set; } = default!;
    }
}
