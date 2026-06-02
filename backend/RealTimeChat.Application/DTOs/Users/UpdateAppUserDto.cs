using Microsoft.AspNetCore.Http;

namespace RealTimeChat.Application.DTOs.Users;

public class UpdateAppUserDto
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public IFormFile? ProfilePicture { get; set; }
}
