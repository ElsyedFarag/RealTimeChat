namespace RealTimeChat.Application.DTOs.Chats;

public class ChatDetailsDto
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? GroupImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ParticipantsCount { get; set; }

    public List<ChatParticipantDto> Participants { get; set; } = [];
}
