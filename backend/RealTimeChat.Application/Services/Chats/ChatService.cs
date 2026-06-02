using AutoMapper;
using RealTimeChat.Application.DTOs.Chats;
using RealTimeChat.Application.Interfaces;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Domain.Enums;
using RealTimeChat.Infrastructure.Repositories.Interfaces;

namespace RealTimeChat.Application.Services.Chats;

public class ChatService : IChatService
{
    private readonly IChatRepository      _chatRepository;
    private readonly IChatParticipantRepository _chatParticipantRepository;
    private readonly IMessageRepository   _messageRepository;
    private readonly IMapper              _mapper;

    public ChatService(
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IMapper mapper,
        IChatParticipantRepository chatParticipantRepository)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
        _chatParticipantRepository = chatParticipantRepository;
    }

    public async Task<IEnumerable<ChatListDto>> GetUserChatsAsync(string userId)
    {
        var chats = await _chatRepository.GetUserChatsOptimizedAsync(userId);

        var result = new List<ChatListDto>();

        foreach (var chat in chats)
        {
            var otherParticipant = chat.Type == ChatType.Private
                ? chat.Participants.FirstOrDefault(p => p.UserId != userId)?.User
                : null;

            var lastMessages = await _messageRepository.GetMessagesByChatIdAsync(chat.Id, 1, 1);
            var lastMessage  = lastMessages.FirstOrDefault();

            var unreadCount  = await _chatRepository.GetUnreadCountAsync(chat.Id, userId);

            result.Add(new ChatListDto
            {
                Id            = chat.Id,
                IsGroup       = chat.Type == ChatType.Group,
                Name          = chat.Type == ChatType.Group ? chat.Name : otherParticipant?.FullName,
                ImageUrl      = otherParticipant?.ProfilePictureUrl,
                CreatedAt     = chat.CreatedAt,
                LastMessage   = lastMessage?.Message,
                LastMessageAt = lastMessage?.SentAt,
                OtherUserId   = otherParticipant?.Id,
                OtherUserName = otherParticipant?.FullName,
                UnreadCount   = unreadCount
            });
        }

        return result.OrderByDescending(c => c.LastMessageAt);
    }

    public async Task<IEnumerable<ChatDto>?> GetAllAsync()
    {
        var chats = await _chatRepository.GetChatsAsync();
        return _mapper.Map<IEnumerable<ChatDto>>(chats);
    }

    public async Task<ChatDetailsDto?> GetByIdAsync(Guid id)
    {
        var chat = await _chatRepository.GetChatWithParticipantsAsync(id);

        if (chat is null)
            return null;

        var dto = _mapper.Map<ChatDetailsDto>(chat);

        // Resolve relative profile picture URLs to absolute
        foreach (var participant in dto.Participants)
            participant.ProfilePictureUrl = participant.ProfilePictureUrl;

        return dto;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WRITE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ChatDto> CreatePrivateChatAsync(
        string currentUserId,
        CreatePrivateChatDto dto)
    {
        if (currentUserId == dto.UserId)
            throw new InvalidOperationException("You cannot create a chat with yourself.");

        var existingChat = await _chatRepository.GetPrivateChatAsync(currentUserId, dto.UserId);

        if (existingChat is not null)
            return _mapper.Map<ChatDto>(existingChat);

        var chat = new Chat { Type = ChatType.Private };

        chat.Participants.Add(new ChatParticipant { UserId = currentUserId });
        chat.Participants.Add(new ChatParticipant { UserId = dto.UserId });

        await _chatRepository.AddAsync(chat);
        await _chatRepository.SaveChangesAsync();

        return _mapper.Map<ChatDto>(chat);
    }

    // ──────────────────────────────────────────────────────────────────────────

    public async Task<ChatDetailsDto> CreateGroupChatAsync(
        string creatorId,
        CreateGroupChatDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Group name is required.");

        if (dto.Name.Length < 2 || dto.Name.Length > 60)
            throw new ArgumentException("Group name must be between 2 and 60 characters.");

        if (dto.MemberIds is null || dto.MemberIds.Count == 0)
            throw new ArgumentException("At least one member is required.");

        // Remove duplicates and the creator themselves (added automatically)
        var memberIds = dto.MemberIds
            .Where(id => !string.IsNullOrWhiteSpace(id) && id != creatorId)
            .Distinct()
            .ToList();

        if (memberIds.Count == 0)
            throw new ArgumentException("At least one other member is required.");

        var chat = new Chat
        {
            Name = dto.Name.Trim(),
            Type = ChatType.Group
        };

        // Creator is always an admin
        chat.Participants.Add(new ChatParticipant
        {
            UserId = creatorId,
            IsAdmin = true
        });

        foreach (var memberId in memberIds)
        {
            chat.Participants.Add(new ChatParticipant
            {
                UserId = memberId,
                IsAdmin = false
            });
        }

        await _chatRepository.AddAsync(chat);
        await _chatRepository.SaveChangesAsync();

        // Return full details (with participants loaded)
        var created = await _chatRepository.GetChatWithParticipantsAsync(chat.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created group.");

        return _mapper.Map<ChatDetailsDto>(created);
    }

    // ──────────────────────────────────────────────────────────────────────────

    public async Task<ChatDetailsDto> AddMembersAsync(
        Guid chatId,
        string requestingUserId,
        AddMembersDto dto)
    {
        var chat = await _chatRepository.GetChatByIdAsync(chatId)
            ?? throw new ArgumentException("Chat not found.");

        if (chat.Type != ChatType.Group)
            throw new InvalidOperationException("Members can only be added to group chats.");

        // Load participants to check admin + avoid duplicates
        var details = await _chatRepository.GetChatWithParticipantsAsync(chatId)!;
        var requestor = details!.Participants.FirstOrDefault(p => p.UserId == requestingUserId);

        if (requestor is null)
            throw new UnauthorizedAccessException("You are not a member of this group.");

        // Re-attach to tracked context for EF to recognise the entity
        var trackedChat = await _chatRepository.GetChatByIdAsync(chatId)!;

        var existingIds = details.Participants.Select(p => p.UserId).ToHashSet();
        var toAdd = (dto.UserIds ?? [])
            .Where(id => !string.IsNullOrWhiteSpace(id) && !existingIds.Contains(id))
            .Distinct()
            .ToList();

        if (toAdd.Count == 0)
            throw new ArgumentException("All specified users are already members.");

        foreach (var uid in toAdd)
        {
            await _chatParticipantRepository.AddAsync(new ChatParticipant
            {
                ChatId = chatId,
                UserId = uid
            });
        }

        await _chatRepository.SaveChangesAsync();

        var updated = await _chatRepository.GetChatWithParticipantsAsync(chatId);
        return _mapper.Map<ChatDetailsDto>(updated!);
    }

    // ──────────────────────────────────────────────────────────────────────────

    public async Task<ChatDetailsDto> RemoveMemberAsync(
        Guid chatId,
        string requestingUserId,
        RemoveMemberDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ArgumentException("UserId is required.");

        var details = await _chatRepository.GetChatWithParticipantsAsync(chatId)
            ?? throw new ArgumentException("Chat not found.");

        if (details.Type != ChatType.Group)
            throw new InvalidOperationException("Members can only be removed from group chats.");

        var requestor = details.Participants.FirstOrDefault(p => p.UserId == requestingUserId);
        if (requestor is null)
            throw new UnauthorizedAccessException("You are not a member of this group.");

        // Only admins can remove others; anyone can remove themselves (leave)
        if (dto.UserId != requestingUserId && !requestor.IsAdmin)
            throw new UnauthorizedAccessException("Only group admins can remove other members.");

        var participant = await _chatParticipantRepository
            .FirstOrDefaultAsync(cp => cp.ChatId == chatId && cp.UserId == dto.UserId);

        if (participant is null)
            throw new ArgumentException("User is not a member of this group.");

        await _chatParticipantRepository.DeleteAsync(participant);
        await _chatParticipantRepository.SaveChangesAsync();

        var updated = await _chatRepository.GetChatWithParticipantsAsync(chatId);
        return _mapper.Map<ChatDetailsDto>(updated!);
    }

    public async Task<bool> DeleteAsync(Guid chatId)
        => await _chatRepository.SoftDeleteAsync(chatId);

}
