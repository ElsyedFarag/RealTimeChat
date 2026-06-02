namespace RealTimeChat.Domain.Entities;

public class MessageReceipt
{
    public Guid MessageId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime? DeliveredAt { get; set; }

    public DateTime? SeenAt { get; set; }

    // Navigation
    public virtual ChatMessage Message { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
