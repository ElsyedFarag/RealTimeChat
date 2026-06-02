namespace RealTimeChat.Application.DTOs.Chats;

public class ChatListDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool IsGroup { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UnreadCount { get; set; }
    // Last message preview
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }

    // Private chat extra
    public string? OtherUserId { get; set; }
    public string? OtherUserName { get; set; }
}