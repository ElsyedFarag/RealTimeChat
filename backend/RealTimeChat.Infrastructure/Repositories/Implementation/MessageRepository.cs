using Microsoft.EntityFrameworkCore;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Infrastructure.Persistence;
using RealTimeChat.Infrastructure.Repositories.Interfaces;

namespace RealTimeChat.Infrastructure.Repositories.Implementation;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // READ
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns paginated messages with Sender loaded for DTO mapping in service.
    /// Ordered DESC so the latest messages come first (typical chat UX).
    /// Index assumption: ChatMessages(ChatId, SentAt DESC) with IsDeleted filter.
    /// </summary>
    public async Task<IEnumerable<ChatMessage>> GetMessagesByChatIdAsync(
        Guid chatId,
        int pageNumber,
        int pageSize)
    {
        return await _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId && !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Sender)
            .ToListAsync();
    }

    /// <summary>
    /// Loads ChatMessage with Sender.
    /// Used when ownership check + full DTO mapping is needed.
    /// </summary>
    public async Task<ChatMessage?> GetMessageByIdAsync(Guid messageId)
    {
        return await _context.ChatMessages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId);
    }

    /// <summary>
    /// Loads only the message scalar columns (no navigations).
    /// Used in SignalR hub for status-only updates to keep payload minimal.
    /// </summary>
    public async Task<ChatMessage?> GetMessageLightAsync(Guid messageId)
    {
        return await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == messageId);
    }

    /// <summary>
    /// Returns receipts with User loaded for DTO mapping.
    /// Index assumption: MessageReceipts(MessageId).
    /// </summary>
    public async Task<IEnumerable<MessageReceipt>> GetReceiptsAsync(Guid messageId)
    {
        return await _context.MessageReceipts
            .AsNoTracking()
            .Where(r => r.MessageId == messageId)
            .Include(r => r.User)
            .ToListAsync();
    }

    /// <summary>
    /// Upserts a MessageReceipt without loading the full ChatMessage entity.
    /// Avoids the N+1 pattern in MarkAsSeenAsync.
    /// Index assumption: MessageReceipts(MessageId, UserId).
    /// </summary>
    public async Task UpsertReceiptAsync(Guid messageId, string userId)
    {
        var receipt = await _context.MessageReceipts
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);

        if (receipt is null)
        {
            _context.MessageReceipts.Add(new MessageReceipt
            {
                MessageId   = messageId,
                UserId      = userId,
                DeliveredAt = DateTime.UtcNow,
                SeenAt      = DateTime.UtcNow
            });
        }
        else
        {
            if (receipt.DeliveredAt is null)
                receipt.DeliveredAt = DateTime.UtcNow;

            receipt.SeenAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Full-text-style search with pagination.
    /// Index assumption: ChatMessages(ChatId) with message text index.
    /// </summary>
    public async Task<IEnumerable<ChatMessage>> SearchMessagesAsync(
        Guid chatId,
        string query,
        int pageNumber,
        int pageSize)
    {
        return await _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId && !m.IsDeleted && m.Message.Contains(query))
            .OrderByDescending(m => m.SentAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Sender)
            .ToListAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WRITE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task AddMessageAsync(ChatMessage message)
        => await _context.ChatMessages.AddAsync(message);

    public Task UpdateMessageAsync(ChatMessage message)
    {
        _context.ChatMessages.Update(message);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
