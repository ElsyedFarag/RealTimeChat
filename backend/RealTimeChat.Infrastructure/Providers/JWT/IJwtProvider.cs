using RealTimeChat.Domain.Entities;
using RealTimeChat.Shared.Responses;

namespace RealTimeChat.Infrastructure.Providers.JWT;

public interface IJwtProvider
{
    Task<JwtResponseDto> GenerateTokenAsync(AppUser user);
    Task<JwtResponseDto> RefreshTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
}
