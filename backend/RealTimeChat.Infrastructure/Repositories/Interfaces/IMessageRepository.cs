using RealTimeChat.Domain.Entities;

namespace RealTimeChat.Infrastructure.Repositories.Interfaces;

public interface IMessageRepository
{
    // === Read ===

    /// <summary>Returns paginated messages with Sender loaded — mapped in service layer.</summary>
    Task<IEnumerable<ChatMessage>> GetMessagesByChatIdAsync(Guid chatId, int pageNumber, int pageSize);

    /// <summary>Returns message with Sender for ownership checks + full DTO mapping.</summary>
    Task<ChatMessage?> GetMessageByIdAsync(Guid messageId);

    /// <summary>Returns message scalar columns only — for status-only updates (Hub, etc.).</summary>
    Task<ChatMessage?> GetMessageLightAsync(Guid messageId);

    /// <summary>Returns receipts with User loaded — mapped to DTO in service layer.</summary>
    Task<IEnumerable<MessageReceipt>> GetReceiptsAsync(Guid messageId);

    /// <summary>Upserts a receipt row without loading the full message entity.</summary>
    Task UpsertReceiptAsync(Guid messageId, string userId);

    /// <summary>Searches messages with pagination; Sender loaded.</summary>
    Task<IEnumerable<ChatMessage>> SearchMessagesAsync(Guid chatId, string query, int pageNumber, int pageSize);

    // === Write ===
    Task AddMessageAsync(ChatMessage message);
    Task UpdateMessageAsync(ChatMessage message);
    Task SaveChangesAsync();
}
