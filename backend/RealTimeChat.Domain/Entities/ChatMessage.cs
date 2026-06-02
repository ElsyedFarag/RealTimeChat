using RealTimeChat.Domain.Enums;

namespace RealTimeChat.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ChatId { get; set; }

    public string SenderId { get; set; } = null!;

    public string Message { get; set; } = null!;

    public MessageType Type { get; set; } = MessageType.Text;
    public MessageStatus Status { get; set; } = MessageStatus.Sent;


    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsEdited { get; set; }

    public DateTime? EditedAt { get; set; }

    public bool IsDeleted { get; set; }

    public string? FileUrl { get; set; }

    // Navigation
    public virtual Chat Chat { get; set; } = null!;

    public virtual AppUser Sender { get; set; } = null!;

    public virtual ICollection<MessageReceipt> Receipts { get; set; }
        = new List<MessageReceipt>();
}
