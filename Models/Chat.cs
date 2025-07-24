using OluBackendApp.Models;

public enum MessageStatus
{
    Sent,
    Delivered,
    Read
}

public class Chat
{
    public int Id { get; set; }
    public string InitiatorId { get; set; } = null!;
    public ApplicationUser Initiator { get; set; } = null!;
    public string RecipientId { get; set; } = null!;
    public ApplicationUser Recipient { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // For “Delete Chat (soft‑delete)”
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}