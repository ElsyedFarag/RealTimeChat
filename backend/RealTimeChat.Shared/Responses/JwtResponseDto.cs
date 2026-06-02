namespace RealTimeChat.Shared.Responses;

public class JwtResponseDto
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
}
