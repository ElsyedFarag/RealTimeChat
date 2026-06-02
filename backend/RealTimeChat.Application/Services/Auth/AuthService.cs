using Microsoft.AspNetCore.Identity;
using RealTimeChat.Application.DTOs.Auth;
using RealTimeChat.Application.Interfaces;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Infrastructure.Providers.JWT;
using RealTimeChat.Shared.Responses;

namespace RealTimeChat.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtProvider _jwtProvider;
    public AuthService(UserManager<AppUser> userManager,
        IJwtProvider jwtProvider)
    {
        _userManager = userManager;
        _jwtProvider = jwtProvider;
    }
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Password is required.");

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            throw new InvalidOperationException("Invalid credentials.");

        var isValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isValid)
            throw new InvalidOperationException("Invalid credentials.");

        var token = await _jwtProvider.GenerateTokenAsync(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            IsAuthenticated = true,
            Token = token.Token,
            RefreshToken = token.RefreshToken,   // ✅
            ExpiresAt = token.ExpiresAt,
            Message = "Login successful."
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.FirstName))
            throw new ArgumentException("First name is required.");
        if (string.IsNullOrWhiteSpace(dto.LastName))
            throw new ArgumentException("Last name is required.");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(dto.UserName))
            throw new ArgumentException("Username is required.");
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Password is required.");

        if (await _userManager.FindByNameAsync(dto.UserName) is not null)
            throw new InvalidOperationException("Username already exists.");

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            throw new InvalidOperationException("Email already exists.");

        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        var tokenResult = await _jwtProvider.GenerateTokenAsync(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            IsAuthenticated = true,
            Token = tokenResult.Token,
            RefreshToken = tokenResult.RefreshToken,
            ExpiresAt = tokenResult.ExpiresAt,
            Message = "User registered successfully."
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            throw new ArgumentException("Refresh token is required.");

        var result = await _jwtProvider.RefreshTokenAsync(dto.Token);

        return new AuthResponseDto
        {
            IsAuthenticated = true,
            Token = result.Token,
            RefreshToken = result.RefreshToken,   
            ExpiresAt = result.ExpiresAt,
            Message = "Token refreshed successfully."
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token is required.");

        return await _jwtProvider.RevokeTokenAsync(refreshToken);
    }
}
