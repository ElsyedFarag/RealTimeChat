using RealTimeChat.Application.DTOs.Chats;

namespace RealTimeChat.Application.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatListDto>> GetUserChatsAsync(string userId);
    Task<IEnumerable<ChatDto>?> GetAllAsync();
    Task<ChatDto> CreatePrivateChatAsync(string currentUserId, CreatePrivateChatDto dto);
    Task<ChatDetailsDto?> GetByIdAsync(Guid id);
    Task<ChatDetailsDto> CreateGroupChatAsync(string creatorId, CreateGroupChatDto dto);

    Task<ChatDetailsDto> AddMembersAsync(Guid chatId, string requestingUserId, AddMembersDto dto);

    Task<ChatDetailsDto> RemoveMemberAsync(Guid chatId, string requestingUserId, RemoveMemberDto dto);
    Task<bool> DeleteAsync(Guid chatId);
}
