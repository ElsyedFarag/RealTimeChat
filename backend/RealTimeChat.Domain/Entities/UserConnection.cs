namespace RealTimeChat.Domain.Entities;

public class UserConnection
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string ConnectionId { get; set; } = null!;

    public DateTime ConnectedAt { get; set; }
}
