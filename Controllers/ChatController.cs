using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Core.Entities;
using backend.DTOs;
using backend.Infrastructure.Services.Contracts;

namespace backend.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupMessages(
    int groupId,
    [FromQuery] int? limit = null,
    [FromQuery] DateTime? before = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));
                _logger.LogInformation($"User {userId} requesting messages for group {groupId}, limit: {limit}, before: {before}");

                if (!await _chatService.IsUserAuthorizedForGroup(userId, groupId))
                {
                    _logger.LogWarning($"User {userId} not authorized for group {groupId}");
                    return Forbid();
                }

                // Pass the userId as currentUserId to get proper read status
                var messages = await _chatService.GetGroupMessages(groupId, limit, before, userId);
                _logger.LogInformation($"Returning {messages.Count} messages for group {groupId}");
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting group messages for groupId: {groupId}");
                return StatusCode(500, new { message = "An error occurred while retrieving messages", details = ex.Message });
            }
        }


        [HttpPost("mark-read/{groupId}")]
        public async Task<IActionResult> MarkMessagesAsRead(int groupId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));
                if (!await _chatService.IsUserAuthorizedForGroup(userId, groupId))
                {
                    return Forbid();
                }

                await _chatService.MarkMessagesAsRead(userId, groupId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return StatusCode(500, new { message = "An error occurred while marking messages as read" });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadMessagesCount()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));
                _logger.LogInformation($"User {userId} requesting unread message count");

                var count = await _chatService.GetUnreadMessagesCount(userId);
                _logger.LogInformation($"Returning unread message count: {count} for user {userId}");

                return Ok(new { count });
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError(sqlEx, $"SQL error getting unread message count: {sqlEx.Message}");

                // Be more specific about CTE errors
                if (sqlEx.Message.Contains("Incorrect syntax near the keyword 'WITH'"))
                {
                    return StatusCode(500, new
                    {
                        message = "SQL syntax error in CTE. Please ensure statements are properly terminated with semicolons.",
                        error = "SQL syntax error"
                    });
                }

                return StatusCode(500, new { message = "A database error occurred", error = "Database error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting unread message count: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving unread count", error = "General error" });
            }
        }


        // Add to ChatController.cs
        [HttpGet("unread-by-group")]
        public async Task<IActionResult> GetUnreadMessagesByGroup()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));
                _logger.LogInformation($"User {userId} requesting unread messages by group");

                var unreadByGroup = await _chatService.GetUnreadMessagesByGroup(userId);
                _logger.LogInformation($"Returning unread messages by group for user {userId}");

                return Ok(unreadByGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting unread messages by group: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving unread messages by group" });
            }
        }

    }
}