using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeChat.Application.DTOs.Users;
using RealTimeChat.Application.Interfaces;
using RealTimeChat.Shared.Responses;
using System.Security.Claims;

namespace RealTimeChat.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await userService.GetAllAsync(pageNumber, pageSize);

            return Ok(ApiResponse<IEnumerable<AppUserDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<AppUserDto>>.Fail(
                "Failed to get users",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(ApiResponse<IEnumerable<AppUserDto>>.Fail("Query is required"));

            var result = await userService.SearchAsync(query);

            return Ok(ApiResponse<IEnumerable<AppUserDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<AppUserDto>>.Fail(
                "Search failed",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var user = await userService.GetByIdAsync(id);

            return user is null
                ? NotFound(ApiResponse<AppUserDto>.Fail("User not found"))
                : Ok(ApiResponse<AppUserDto>.Ok(user));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<AppUserDto>.Fail(
                "Failed to get user",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        try
        {
            var userId = GetUserId();

            var user = await userService.GetByIdAsync(userId);

            return user is null
                ? NotFound(ApiResponse<AppUserDto>.Fail("User not found"))
                : Ok(ApiResponse<AppUserDto>.Ok(user));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AppUserDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<AppUserDto>.Fail(
                "Failed to get current user",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromForm] UpdateAppUserDto dto)
    {
        try
        {
            var result = await userService.UpdateUserAsync(id, dto);

            return result is null
                ? NotFound(ApiResponse<AppUserDto>.Fail("User not found"))
                : Ok(ApiResponse<AppUserDto>.Ok(result, "Updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AppUserDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<AppUserDto>.Fail(
                "Update failed",
                new List<string> { ex.Message }
            ));
        }
    }
}