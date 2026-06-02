namespace RealTimeChat.Application.DTOs.Users;

public class AppUserDto
{
    public string Id { get; set; } = null!;

    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName => $"{FirstName} {LastName}";

    public string? ProfilePictureUrl { get; set; }

    public bool IsOnline { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastSeenAt { get; set; }
}
