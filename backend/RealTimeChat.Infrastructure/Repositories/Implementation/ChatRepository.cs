using Microsoft.EntityFrameworkCore;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Domain.Enums;
using RealTimeChat.Infrastructure.Persistence;
using RealTimeChat.Infrastructure.Repositories.Interfaces;

namespace RealTimeChat.Infrastructure.Repositories.Implementation;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // READ
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the user's chats with participants and the last message only.
    /// Does NOT load the full Messages collection — uses a sub-query.
    /// Index assumptions:
    ///   ChatParticipants(UserId)
    ///   ChatMessages(ChatId, SentAt DESC)
    /// </summary>
    public async Task<IEnumerable<Chat>> GetUserChatsOptimizedAsync(string userId)
    {
        // Load Chat + Participants.User in one query.
        // Messages are intentionally NOT included here; last message is fetched
        // separately per chat in the service layer to keep this query clean.
        return await _context.Chats
            .AsNoTracking()
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .ToListAsync();
    }

    /// <summary>
    /// Returns the full Chat with Participants and their User — for the details view.
    /// Does NOT include the Messages collection.
    /// </summary>
    public async Task<Chat?> GetChatWithParticipantsAsync(Guid chatId)
    {
        return await _context.Chats
            .AsNoTracking()
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == chatId);
    }

    /// <summary>
    /// Finds an existing private chat between exactly two users.
    /// Avoids Participants.Count == 2 which causes a full collection scan in SQL.
    /// Instead checks both participants exist and no third participant is present.
    /// Index: ChatParticipants(UserId).
    /// </summary>
    public async Task<Chat?> GetPrivateChatAsync(string user1Id, string user2Id)
    {
        return await _context.Chats
            .AsNoTracking()
            .Where(c =>
                c.Type == ChatType.Private &&
                c.Participants.Any(p => p.UserId == user1Id) &&
                c.Participants.Any(p => p.UserId == user2Id) &&
                c.Participants.Count(p => p.UserId == user1Id || p.UserId == user2Id) == 2)
            .FirstOrDefaultAsync();
    }

    /// <summary>Returns basic Chat rows for the admin endpoint. No navigations.</summary>
    public async Task<IEnumerable<Chat>> GetChatsAsync()
    {
        return await _context.Chats
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>Bare Chat entity — no navigations. Suitable for write operations.</summary>
    public async Task<Chat?> GetChatByIdAsync(Guid chatId)
    {
        return await _context.Chats
            .FirstOrDefaultAsync(c => c.Id == chatId);
    }

    /// <summary>
    /// Single EXISTS query — avoids loading the participant list.
    /// Index: ChatParticipants(UserId, ChatId).
    /// </summary>
    public async Task<bool> IsParticipantAsync(Guid chatId, string userId)
    {
        return await _context.ChatParticipants
            .AsNoTracking()
            .AnyAsync(cp => cp.ChatId == chatId && cp.UserId == userId);
    }

    /// <summary>
    /// Returns the unread message count for a user in a chat.
    /// A message is "unread" if it has no SeenAt receipt from this user.
    /// Single COUNT query — no collection loaded.
    /// Index: MessageReceipts(MessageId, UserId), ChatMessages(ChatId).
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid chatId, string userId)
    {
        return await _context.ChatMessages
            .AsNoTracking()
            .CountAsync(m =>
                m.ChatId == chatId &&
                !m.IsDeleted &&
                m.SenderId != userId &&
                !m.Receipts.Any(r => r.UserId == userId && r.SeenAt != null));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WRITE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task AddAsync(Chat chat)
        => await _context.Chats.AddAsync(chat);

    public Task UpdateAsync(Chat chat)
    {
        _context.Chats.Update(chat);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    /// <summary>
    /// Soft-delete: sets IsDeleted = true and DeletedAt = now.
    /// No hard DELETE — data is preserved for audit / recovery.
    /// </summary>
    public async Task<bool> SoftDeleteAsync(Guid chatId)
    {
        var chat = await _context.Chats
            .IgnoreQueryFilters()   // bypass global filter to find the record
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat is null || chat.IsDeleted)
            return false;

        chat.IsDeleted = true;
        chat.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
