namespace RealTimeChat.Application.DTOs.Messages;
public class MessageDto
{
    public Guid Id { get; set; }

    public Guid ChatId { get; set; }

    public string SenderId { get; set; } = null!;

    public string SenderName { get; set; } = null!;

    public string Message { get; set; } = null!;
    public string Type { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public bool IsEdited { get; set; }

    public bool IsDeleted { get; set; }

    public string Status { get; set; } = "Sent";
}