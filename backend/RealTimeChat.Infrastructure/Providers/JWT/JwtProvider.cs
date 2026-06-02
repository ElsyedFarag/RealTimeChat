using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Infrastructure.Persistence;
using RealTimeChat.Infrastructure.Providers.JWT;
using RealTimeChat.Shared.Responses;
using RealTimeChat.Shared.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class JwtProvider : IJwtProvider
{
    private readonly JwtSettings _jwtSettings;
    private readonly AppDbContext _context;

    public JwtProvider(
        IOptions<JwtSettings> options,
        AppDbContext context)
    {
        _jwtSettings = options.Value;
        _context = context;
    }

    public async Task<JwtResponseDto> GenerateTokenAsync(AppUser user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(_jwtSettings.Key))
            throw new InvalidOperationException("JWT Key is missing.");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),

            new("firstName", user.FirstName),
            new("lastName", user.LastName),

            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = GenerateRefreshToken();

        var refreshEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            IsUsed = false
        };

        await _context.RefreshTokens.AddAsync(refreshEntity);
        await _context.SaveChangesAsync();

        return new JwtResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expires
        };
    }
    public async Task<JwtResponseDto> RefreshTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required.");

        var refreshToken = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);

        if (refreshToken is null)
            throw new InvalidOperationException("Invalid refresh token.");

        if (refreshToken.IsUsed || refreshToken.IsRevoked)
            throw new InvalidOperationException("Refresh token is not valid.");

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Refresh token expired.");

        refreshToken.IsUsed = true;
        return await GenerateTokenAsync(refreshToken.User);
    }
    public async Task<bool> RevokeTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token);

        if (refreshToken is null)
            return false;

        refreshToken.IsRevoked = true;

        await _context.SaveChangesAsync();

        return true;
    }
    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}