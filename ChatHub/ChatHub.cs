using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using backend.Infrastructure.Services.Contracts;
using backend.DTOs;
using System.Security.Claims;

namespace backend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            // Get the user's groups and join them
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            _logger.LogInformation($"User: {Context.User?.Identity?.Name ?? "Unknown"}");

            var userId = int.Parse(Context.User.FindFirstValue("UserId"));
            var groups = await _chatService.GetUserGroupIds(userId);
            // ...
            foreach (var groupId in groups)
            {
                // Add connection to group - group name is "group_{id}"
                await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
            }

            await base.OnConnectedAsync();
        }

        public async Task JoinGroup(int groupId)
        {
            // Validate user belongs to group
            var userId = int.Parse(Context.User.FindFirstValue("UserId"));
            if (await _chatService.IsUserAuthorizedForGroup(userId, groupId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");

                // Get messages with current user ID to correctly calculate read status
                var messages = await _chatService.GetGroupMessages(groupId, null, null, userId);

                await Clients.Caller.SendAsync("JoinedGroup", groupId, messages);
            }
        }

        public async Task LeaveGroup(int groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupId}");
        }

        // Fix for ChatHub.cs - SendMessageToGroup method
        public async Task SendMessageToGroup(int groupId, string content)
        {
            try
            {
                var context = Context.GetHttpContext();
                if (context == null)
                {
                    throw new Exception("HTTP context not available");
                }

                var userIdClaim = context.User.FindFirst("UserId");
                if (userIdClaim == null)
                {
                    throw new Exception("User ID not found in claims");
                }

                var userId = int.Parse(userIdClaim.Value);

                // Check authorization
                if (!await _chatService.IsUserAuthorizedForGroup(userId, groupId))
                {
                    throw new Exception("User not authorized for this group");
                }

                // Send and save the message
                var messageDto = await _chatService.SendMessage(userId, groupId, content);

                // FIX: Use consistent group naming convention
                await Clients.Group($"group_{groupId}").SendAsync("ReceiveMessage", messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessageToGroup");
                throw;
            }
        }
        public async Task MarkMessagesAsRead(int groupId)
        {
            var userId = int.Parse(Context.User.FindFirstValue("UserId"));
            if (await _chatService.IsUserAuthorizedForGroup(userId, groupId))
            {
                await _chatService.MarkMessagesAsRead(userId, groupId);

                // Notify group that user has seen messages
                await Clients.Group($"group_{groupId}").SendAsync("MessagesRead", userId, groupId);
            }
        }

    
        public async Task NotifyTyping(int groupId)
        {
            var userId = int.Parse(Context.User.FindFirstValue("UserId"));
            var userName = Context.User.FindFirstValue("name") ?? Context.User.FindFirstValue(ClaimTypes.Name);

            if (await _chatService.IsUserAuthorizedForGroup(userId, groupId))
            {
                await Clients.OthersInGroup($"group_{groupId}").SendAsync("UserTyping", userId, userName, groupId);
            }
        }

        public Task Ping()
        {
            // Simple ping method to keep the connection alive
            return Task.CompletedTask;
        }
    }
}