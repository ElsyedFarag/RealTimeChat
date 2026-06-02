using Microsoft.AspNetCore.Identity;

namespace RealTimeChat.Domain.Entities;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public string FullName => $"{FirstName} {LastName}";

    public bool IsOnline { get; set; } = false;

    public string? ProfilePictureUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastSeenAt { get; set; }

    // Navigation
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    = new List<RefreshToken>();

    public virtual ICollection<ChatParticipant> ChatParticipants { get; set; }
        = new List<ChatParticipant>();

    public virtual ICollection<ChatMessage> SentMessages { get; set; }
        = new List<ChatMessage>();
}
