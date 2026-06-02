using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeChat.Application.DTOs.Chats;
using RealTimeChat.Application.Interfaces;
using RealTimeChat.Shared.Responses;
using System.Security.Claims;

namespace RealTimeChat.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ChatsController(IChatService chatService) : ControllerBase
{
    private string? UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await chatService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ChatDto>?>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<ChatDto>>.Fail(
                "Failed to retrieve chats",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyChats()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(UserId))
                return Unauthorized(ApiResponse<IEnumerable<ChatListDto>>.Fail(
                    "User not authenticated"
                ));

            var result = await chatService.GetUserChatsAsync(UserId);

            return Ok(ApiResponse<IEnumerable<ChatListDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<ChatListDto>>.Fail(
                "Failed to get user chats",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize]
    [HttpPost("private")]
    public async Task<IActionResult> CreatePrivate(CreatePrivateChatDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(UserId))
                return Unauthorized(ApiResponse<ChatDto>.Fail(
                    "User not authenticated"
                ));

            var chat = await chatService.CreatePrivateChatAsync(UserId, dto);

            return Ok(ApiResponse<ChatDto>.Ok(
                chat,
                "Chat created successfully"
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ChatDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ChatDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ChatDto>.Fail(
                "Failed to create chat",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var chat = await chatService.GetByIdAsync(id);

            return chat is null
                ? NotFound(ApiResponse<ChatDetailsDto>.Fail("Chat not found"))
                : Ok(ApiResponse<ChatDetailsDto>.Ok(chat));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ChatDetailsDto>.Fail(
                "Failed to get chat",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize]
    [HttpPost("group")]
    public async Task<IActionResult> CreateGroup(CreateGroupChatDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(UserId))
                return Unauthorized(ApiResponse<ChatDetailsDto>.Fail("User not authenticated"));

            var chat = await chatService.CreateGroupChatAsync(UserId, dto);

            return Ok(ApiResponse<ChatDetailsDto>.Ok(chat, "Group created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ChatDetailsDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ChatDetailsDto>.Fail(
                "Failed to create group",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize]
    [HttpPatch("{id:guid}/members/add")]
    public async Task<IActionResult> AddMembers(Guid id, AddMembersDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(UserId))
                return Unauthorized(ApiResponse<ChatDetailsDto>.Fail("User not authenticated"));

            var result = await chatService.AddMembersAsync(id, UserId, dto);

            return Ok(ApiResponse<ChatDetailsDto>.Ok(result, "Members added successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ChatDetailsDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ChatDetailsDto>.Fail(
                "Failed to add members",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize]
    [HttpPatch("{id:guid}/members/remove")]
    public async Task<IActionResult> RemoveMember(Guid id, RemoveMemberDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(UserId))
                return Unauthorized(ApiResponse<ChatDetailsDto>.Fail("User not authenticated"));

            var result = await chatService.RemoveMemberAsync(id, UserId, dto);

            return Ok(ApiResponse<ChatDetailsDto>.Ok(result, "Member removed successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ChatDetailsDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ChatDetailsDto>.Fail(
                "Failed to remove member",
                new List<string> { ex.Message }
            ));
        }
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await chatService.DeleteAsync(id);

            return result
                ? Ok(ApiResponse<string>.Ok("Chat deleted successfully"))
                : NotFound(ApiResponse<string>.Fail("Chat not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail(
                "Failed to delete chat",
                new List<string> { ex.Message }
            ));
        }
    }
}