namespace RealTimeChat.Domain.Entities;

public class ChatParticipant
{
    public Guid ChatId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool IsAdmin { get; set; } = false;

    // Navigation
    public virtual Chat Chat { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
