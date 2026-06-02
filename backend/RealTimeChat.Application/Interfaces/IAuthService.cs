using RealTimeChat.Application.DTOs.Auth;
using RealTimeChat.Shared.Responses;

namespace RealTimeChat.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);

    Task<bool> RevokeTokenAsync(string refreshToken);
}