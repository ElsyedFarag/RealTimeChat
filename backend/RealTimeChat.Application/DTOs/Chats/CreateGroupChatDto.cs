namespace RealTimeChat.Application.DTOs.Chats;

public class CreateGroupChatDto
{
    public string Name { get; set; } = null!;
    public List<string> MemberIds { get; set; } = [];
}
