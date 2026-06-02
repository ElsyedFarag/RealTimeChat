namespace RealTimeChat.Shared.Responses;

public class AuthResponseDto
{
    public bool IsAuthenticated { get; set; }

    public string Message { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;
}