using Microsoft.AspNetCore.Mvc;
using RealTimeChat.Application.DTOs.Auth;
using RealTimeChat.Application.Interfaces;
using RealTimeChat.Shared.Responses;

namespace RealTimeChat.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            var result = await authService.RegisterAsync(dto);

            return Ok(ApiResponse<AuthResponseDto>.Ok(
                result,
                "User registered successfully"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(
                ex.Message
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(
                ex.Message
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<AuthResponseDto>.Fail(
                "Unexpected error occurred",
                new List<string> { ex.Message }
            ));
        }
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            var result = await authService.LoginAsync(dto);

            return Ok(ApiResponse<AuthResponseDto>.Ok(
                result,
                "Login successful"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<AuthResponseDto>.Fail(
                "Unexpected error",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
    {
        try
        {
            var result = await authService.RefreshTokenAsync(dto);

            return Ok(ApiResponse<AuthResponseDto>.Ok(
                result,
                "Token refreshed successfully"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<AuthResponseDto>.Fail(
                "Unexpected error",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken(RevokeTokenDto dto)
    {
        try
        {
            var revoked = await authService.RevokeTokenAsync(dto.Token);

            return revoked
                ? Ok(ApiResponse<object>.Ok(true, "Token revoked successfully"))
                : NotFound(ApiResponse<object>.Fail("Token not found"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail(
                "Unexpected error",
                new List<string> { ex.Message }
            ));
        }
    }
}