using RealTimeChat.Domain.Enums;

namespace RealTimeChat.Domain.Entities;

public class Chat
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Name { get; set; }

    /// <summary>Group / Private</summary>
    public ChatType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Soft-delete ──────────────────────────────────────────────────────────
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public virtual ICollection<ChatParticipant> Participants { get; set; }
        = new List<ChatParticipant>();

    public virtual ICollection<ChatMessage> Messages { get; set; }
        = new List<ChatMessage>();
}
