using RealTimeChat.Domain.Entities;

namespace RealTimeChat.Infrastructure.Repositories.Interfaces;

public interface IChatRepository
{
    // === Optimized read queries ===

    /// <summary>Returns chats with minimal data needed for the sidebar list. No Messages included.</summary>
    Task<IEnumerable<Chat>> GetUserChatsOptimizedAsync(string userId);

    /// <summary>Returns full Chat with Participants.User loaded — for details view.</summary>
    Task<Chat?> GetChatWithParticipantsAsync(Guid chatId);

    /// <summary>Optimized private-chat lookup — does not use Participants.Count == 2 in EF.</summary>
    Task<Chat?> GetPrivateChatAsync(string user1Id, string user2Id);

    /// <summary>Returns basic Chat rows for admin list — no navigation props.</summary>
    Task<IEnumerable<Chat>> GetChatsAsync();

    /// <summary>Returns bare Chat entity (no navigations) — for write operations.</summary>
    Task<Chat?> GetChatByIdAsync(Guid chatId);

    /// <summary>Single EXISTS query — does not load the participant list.</summary>
    Task<bool> IsParticipantAsync(Guid chatId, string userId);

    /// <summary>Returns the unread message count for a user in a chat. Single COUNT query.</summary>
    Task<int> GetUnreadCountAsync(Guid chatId, string userId);

    // === Write ===
    Task AddAsync(Chat chat);
    Task UpdateAsync(Chat chat);
    Task SaveChangesAsync();

    /// <summary>Soft-delete: sets IsDeleted + DeletedAt. No hard DELETE.</summary>
    Task<bool> SoftDeleteAsync(Guid chatId);
}
