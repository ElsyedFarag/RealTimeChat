namespace RealTimeChat.Application.DTOs.Chats;

public class ChatParticipantDto
{
    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? ProfilePictureUrl { get; set; }

    public bool IsOnline { get; set; }
}
