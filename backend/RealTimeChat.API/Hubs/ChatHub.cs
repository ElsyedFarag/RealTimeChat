using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using RealTimeChat.Application.DTOs.Messages;
using RealTimeChat.Application.Interfaces;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Domain.Enums;
using RealTimeChat.Infrastructure.Repositories.Interfaces;

namespace RealTimeChat.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly IChatRepository _chatRepository;
    private readonly IUserConnectionRepository _connectionRepository;
    private readonly UserManager<AppUser> _userManager;

    public ChatHub(
        IMessageService messageService,
        IChatRepository chatRepository,
        IUserConnectionRepository connectionRepository,
        UserManager<AppUser> userManager)
    {
        _messageService = messageService;
        _chatRepository = chatRepository;
        _connectionRepository = connectionRepository;
        _userManager = userManager;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CONNECTION LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user is not null)
        {
            // Track connection
            await _connectionRepository.AddAsync(new UserConnection
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                ConnectedAt = DateTime.UtcNow
            });
            await _connectionRepository.SaveChangesAsync();

            // Mark user as online
            user.IsOnline = true;
            await _userManager.UpdateAsync(user);

            // Join all chat rooms this user belongs to
            var chats = await _chatRepository.GetUserChatsOptimizedAsync(userId);
            foreach (var chat in chats)
                await Groups.AddToGroupAsync(Context.ConnectionId, chat.Id.ToString());

            // Notify others that this user is now online
            await Clients.Others.SendAsync(HubEvents.UserOnline, new
            {
                UserId = userId,
                FullName = user.FullName,
                IsOnline = true
            });
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();

        await _connectionRepository.RemoveAsync(Context.ConnectionId);
        await _connectionRepository.SaveChangesAsync();

        var remainingConnections = await _connectionRepository.GetConnectionCountAsync(userId);

        // Only mark offline when ALL tabs/devices are disconnected
        if (remainingConnections == 0)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is not null)
            {
                user.IsOnline = false;
                user.LastSeenAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                await Clients.Others.SendAsync(HubEvents.UserOffline, new
                {
                    UserId = userId,
                    LastSeenAt = user.LastSeenAt
                });
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MESSAGING
    // ─────────────────────────────────────────────────────────────────────────
    public async Task SendMessage(SendMessageDto dto)
    {
        var senderId = GetUserId();

        try
        {
            var message = await _messageService.SendMessageAsync(senderId, dto);

            // Broadcast to everyone in the chat group (including sender)
            await Clients
                .Group(dto.ChatId.ToString())
                .SendAsync(HubEvents.ReceiveMessage, message);

            // Update status to Delivered for all online members
            await NotifyDeliveredAsync(dto.ChatId, message.Id);
        }
        catch (UnauthorizedAccessException ex)
        {
            await Clients.Caller.SendAsync(HubEvents.Error, ex.Message);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync(HubEvents.Error, "Failed to send message: " + ex.Message);
        }
    }

    public async Task EditMessage(Guid messageId, string newContent)
    {
        var userId = GetUserId();

        try
        {
            var updated = await _messageService.UpdateMessageAsync(
                messageId,
                userId,
                new UpdateMessageDto { Message = newContent });

            await Clients
                .Group(updated.ChatId.ToString())
                .SendAsync(HubEvents.MessageEdited, updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            await Clients.Caller.SendAsync(HubEvents.Error, ex.Message);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync(HubEvents.Error, "Failed to edit message: " + ex.Message);
        }
    }

    public async Task DeleteMessage(Guid messageId, Guid chatId)
    {
        var userId = GetUserId();

        try
        {
            var deleted = await _messageService.DeleteMessageAsync(messageId, userId);

            if (deleted)
            {
                await Clients
                    .Group(chatId.ToString())
                    .SendAsync(HubEvents.MessageDeleted, new { MessageId = messageId, ChatId = chatId });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            await Clients.Caller.SendAsync(HubEvents.Error, ex.Message);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync(HubEvents.Error, "Failed to delete message: " + ex.Message);
        }
    }

    public async Task MarkAsSeen(Guid messageId, Guid chatId)
    {
        var userId = GetUserId();

        try
        {
            await _messageService.MarkAsSeenAsync(messageId, userId);
            await _messageService.UpdateMessageStatusAsync(messageId, MessageStatus.Seen);

            await Clients
                .Group(chatId.ToString())
                .SendAsync(HubEvents.MessageSeen, new
                {
                    MessageId = messageId,
                    ChatId = chatId,
                    SeenBy = userId,
                    SeenAt = DateTime.UtcNow
                });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync(HubEvents.Error, "Failed to mark as seen: " + ex.Message);
        }
    }


    /// <summary>Client calls this while the user is typing.</summary>
    public async Task StartTyping(Guid chatId)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        // Broadcast to everyone in the chat EXCEPT the typer
        await Clients
            .GroupExcept(chatId.ToString(), Context.ConnectionId)
            .SendAsync(HubEvents.UserTyping, new
            {
                ChatId = chatId,
                UserId = userId,
                FullName = user?.FullName
            });
    }

    /// <summary>Client calls this when the user stops typing.</summary>
    public async Task StopTyping(Guid chatId)
    {
        var userId = GetUserId();

        await Clients
            .GroupExcept(chatId.ToString(), Context.ConnectionId)
            .SendAsync(HubEvents.UserStoppedTyping, new
            {
                ChatId = chatId,
                UserId = userId
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CHAT ROOM MANAGEMENT
    // ─────────────────────────────────────────────────────────────────────────

    public async Task JoinChat(Guid chatId)
    {
        var userId = GetUserId();
        var isParticipant = await _chatRepository.IsParticipantAsync(chatId, userId);

        if (!isParticipant)
        {
            await Clients.Caller.SendAsync(HubEvents.Error, "You are not a participant in this chat.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        await Clients.Caller.SendAsync(HubEvents.JoinedChat, chatId);
    }

    /// <summary>Leave a chat group (e.g. user was removed from a group chat).</summary>
    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        await Clients.Caller.SendAsync(HubEvents.LeftChat, chatId);
    }


    public async Task NotifyGroupCreated(Guid chatId, object chatSummary, List<string> memberIds)
    {
        foreach (var memberId in memberIds)
        {
            // Notify each member's active connections
            await Clients.User(memberId).SendAsync(HubEvents.GroupCreated, chatSummary);
        }

        // Add every current connection to the new group
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task NotifyMemberAdded(Guid chatId, string newUserId, object memberDto)
    {
        // Tell everyone already in the group
        await Clients
            .Group(chatId.ToString())
            .SendAsync(HubEvents.MemberAdded, new { ChatId = chatId, Member = memberDto });

        // Pull new member's connections into the SignalR group
        await Clients.User(newUserId).SendAsync(HubEvents.GroupCreated, new { Id = chatId });
    }

    public async Task NotifyMemberRemoved(Guid chatId, string removedUserId)
    {
        await Clients
            .Group(chatId.ToString())
            .SendAsync(HubEvents.MemberRemoved, new { ChatId = chatId, UserId = removedUserId });
    }
    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    private async Task NotifyDeliveredAsync(Guid chatId, Guid messageId)
    {
        await _messageService.UpdateMessageStatusAsync(messageId, MessageStatus.Delivered);

        await Clients
            .Group(chatId.ToString())
            .SendAsync(HubEvents.MessageDelivered, new
            {
                MessageId = messageId,
                ChatId = chatId,
                DeliveredAt = DateTime.UtcNow
            });
    }

    private string GetUserId() =>
        Context.UserIdentifier
        ?? throw new HubException("User is not authenticated.");
}