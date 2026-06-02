namespace RealTimeChat.Application.DTOs.Messages;

public class MessageReceiptDto
{
    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public DateTime? DeliveredAt { get; set; }

    public DateTime? SeenAt { get; set; }
}
