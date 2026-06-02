namespace RealTimeChat.Application.DTOs.Messages
{
    public class SendMessageDto
    {
        public Guid ChatId { get; set; }

        public string Message { get; set; } = null!;
    }
}
