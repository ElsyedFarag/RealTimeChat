using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeChat.Application.DTOs.Messages;
using RealTimeChat.Application.Interfaces;
using RealTimeChat.Shared.Responses;
using System.Security.Claims;

namespace RealTimeChat.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class MessagesController(IMessageService messageService) : ControllerBase
{
    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated");
    }

    [HttpPost]
    public async Task<IActionResult> Send(SendMessageDto dto)
    {
        try
        {
            var userId = GetUserId();

            var result = await messageService.SendMessageAsync(userId, dto);

            return Ok(ApiResponse<MessageDto>.Ok(result, "Message sent"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<MessageDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<MessageDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<MessageDto>.Fail(
                "Failed to send message",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var msg = await messageService.GetByIdAsync(id);

            return msg is null
                ? NotFound(ApiResponse<MessageDto>.Fail("Message not found"))
                : Ok(ApiResponse<MessageDto>.Ok(msg));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<MessageDto>.Fail(
                "Failed to get message",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("chat/{chatId:guid}")]
    public async Task<IActionResult> GetChatMessages(
        Guid chatId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await messageService
                .GetMessagesByChatIdAsync(chatId, pageNumber, pageSize);

            return Ok(ApiResponse<IEnumerable<MessageDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<MessageDto>>.Fail(
                "Failed to get messages",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMessageDto dto)
    {
        try
        {
            var userId = GetUserId();

            var result = await messageService.UpdateMessageAsync(id, userId, dto);

            return Ok(ApiResponse<MessageDto>.Ok(result, "Message updated"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<MessageDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<MessageDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<MessageDto>.Fail(
                "Failed to update message",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();

            var result = await messageService.DeleteMessageAsync(id, userId);

            return result
                ? Ok(ApiResponse<bool>.Ok(true, "Message deleted"))
                : NotFound(ApiResponse<bool>.Fail("Message not found"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<bool>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.Fail(
                "Failed to delete message",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpPost("{id:guid}/seen")]
    public async Task<IActionResult> Seen(Guid id)
    {
        try
        {
            var userId = GetUserId();

            await messageService.MarkAsSeenAsync(id, userId);

            return Ok(ApiResponse<string>.Ok("Marked as seen"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail(
                "Failed to mark message as seen",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpGet("{id:guid}/receipts")]
    public async Task<IActionResult> Receipts(Guid id)
    {
        try
        {
            var result = await messageService.GetReceiptsAsync(id);

            return Ok(ApiResponse<IEnumerable<MessageReceiptDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<MessageReceiptDto>>.Fail(
                "Failed to get receipts",
                new List<string> { ex.Message }
            ));
        }
    }
}