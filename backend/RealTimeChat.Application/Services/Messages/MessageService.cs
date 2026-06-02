using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RealTimeChat.Application.DTOs.Messages;
using RealTimeChat.Application.Interfaces;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Domain.Enums;
using RealTimeChat.Infrastructure.Repositories.Interfaces;

namespace RealTimeChat.Application.Services.Messages;

public class MessageService : IMessageService
{
    private readonly IMessageRepository   _messageRepository;
    private readonly IChatRepository      _chatRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper              _mapper;

    public MessageService(
        IMessageRepository messageRepository,
        IChatRepository chatRepository,
        UserManager<AppUser> userManager,
        IMapper mapper)
    {
        _messageRepository = messageRepository;
        _chatRepository    = chatRepository;
        _userManager       = userManager;
        _mapper            = mapper;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // READ
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<MessageDto?> GetByIdAsync(Guid messageId)
    {
        var message = await _messageRepository.GetMessageByIdAsync(messageId);
        return message is null ? null : _mapper.Map<MessageDto>(message);
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesByChatIdAsync(
        Guid chatId,
        int pageNumber = 1,
        int pageSize   = 20)
    {
        var messages = await _messageRepository.GetMessagesByChatIdAsync(chatId, pageNumber, pageSize);
        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<IEnumerable<MessageReceiptDto>> GetReceiptsAsync(Guid messageId)
    {
        var receipts = await _messageRepository.GetReceiptsAsync(messageId);
        return _mapper.Map<IEnumerable<MessageReceiptDto>>(receipts);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WRITE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<MessageDto> SendMessageAsync(string senderId, SendMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Message))
            throw new ArgumentException("Message cannot be empty.");

        // Single EXISTS query — no full chat entity loaded
        var isParticipant = await _chatRepository.IsParticipantAsync(dto.ChatId, senderId);
        if (!isParticipant)
            throw new UnauthorizedAccessException("You are not a participant in this chat.");

        // Verify chat exists (lightweight scalar fetch)
        var chat = await _chatRepository.GetChatByIdAsync(dto.ChatId);
        if (chat is null)
            throw new InvalidOperationException("Chat not found.");

        var user = await _userManager.FindByIdAsync(senderId)
            ?? throw new InvalidOperationException("Sender not found.");

        var message = new ChatMessage
        {
            ChatId    = dto.ChatId,
            SenderId  = senderId,
            Message   = dto.Message,
            Type      = MessageType.Text,
            Status    = MessageStatus.Sent,
            SentAt    = DateTime.UtcNow,
            IsDeleted = false
        };

        await _messageRepository.AddMessageAsync(message);
        await _messageRepository.SaveChangesAsync();

        // Map then override SenderName — Sender nav prop not loaded after insert
        var result = _mapper.Map<MessageDto>(message);
        result.SenderName = user.FullName;
        return result;
    }

    public async Task<MessageDto> UpdateMessageAsync(
        Guid messageId,
        string userId,
        UpdateMessageDto dto)
    {
        var message = await _messageRepository.GetMessageByIdAsync(messageId)
            ?? throw new InvalidOperationException("Message not found.");

        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("You cannot edit this message.");

        message.Message  = dto.Message;
        message.IsEdited = true;
        message.EditedAt = DateTime.UtcNow;

        await _messageRepository.UpdateMessageAsync(message);
        await _messageRepository.SaveChangesAsync();

        return _mapper.Map<MessageDto>(message);
    }

    /// <summary>Soft-delete: IsDeleted = true (schema already supports this).</summary>
    public async Task<bool> DeleteMessageAsync(Guid messageId, string userId)
    {
        var message = await _messageRepository.GetMessageByIdAsync(messageId);
        if (message is null) return false;

        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("You cannot delete this message.");

        message.IsDeleted = true;
        message.EditedAt  = DateTime.UtcNow;

        await _messageRepository.UpdateMessageAsync(message);
        await _messageRepository.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Marks a message as seen.
    /// Uses a light load to confirm message existence, then upserts the receipt row.
    /// Does NOT load the full ChatMessage entity graph.
    /// </summary>
    public async Task MarkAsSeenAsync(Guid messageId, string userId)
    {
        var exists = await _messageRepository.GetMessageLightAsync(messageId);
        if (exists is null)
            throw new InvalidOperationException("Message not found.");

        await _messageRepository.UpsertReceiptAsync(messageId, userId);
    }

    /// <summary>
    /// Updates aggregate MessageStatus on the ChatMessage row.
    /// Uses GetMessageLightAsync — no navigation properties loaded.
    /// </summary>
    public async Task UpdateMessageStatusAsync(Guid messageId, MessageStatus status)
    {
        var message = await _messageRepository.GetMessageLightAsync(messageId)
            ?? throw new InvalidOperationException("Message not found.");

        message.Status = status;

        await _messageRepository.UpdateMessageAsync(message);
        await _messageRepository.SaveChangesAsync();
    }
}
