using RealTimeChat.Application.DTOs.Messages;
using RealTimeChat.Domain.Enums;

namespace RealTimeChat.Application.Interfaces;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(
        string senderId,
        SendMessageDto dto);

    Task<MessageDto?> GetByIdAsync(
        Guid messageId);

    Task<IEnumerable<MessageDto>> GetMessagesByChatIdAsync(
        Guid chatId,
        int pageNumber = 1,
        int pageSize = 20);

    Task<MessageDto> UpdateMessageAsync(
        Guid messageId,
        string userId,
        UpdateMessageDto dto);

    Task<bool> DeleteMessageAsync(
        Guid messageId,
        string userId);

    Task MarkAsSeenAsync(Guid messageId,
        string userId);
    Task<IEnumerable<MessageReceiptDto>> GetReceiptsAsync(
        Guid messageId);

    Task UpdateMessageStatusAsync(Guid messageId,
        MessageStatus status);
}