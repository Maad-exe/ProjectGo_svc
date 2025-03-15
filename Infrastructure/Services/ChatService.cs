using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Enums;
using backend.DTOs;
using backend.Infrastructure.Data.UnitOfWork.Contract;
using backend.Infrastructure.Services.Contracts;

namespace backend.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ChatMessageDto>> GetGroupMessages(int groupId, int? limit = null, DateTime? before = null)
        {
            var messages = await _unitOfWork.Chat.GetMessagesForGroupAsync(groupId, limit, before);

            // Log the message count for debugging
            Console.WriteLine($"Retrieved {messages.Count} messages for group {groupId}");

            var dtos = messages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                GroupId = m.GroupId,
                SenderId = m.SenderId,
                SenderName = m.Sender?.FullName ?? "Unknown User", // Handle null sender
                SenderRole = m.Sender?.Role.ToString() ?? "Unknown",
                Content = m.Content,
                Timestamp = m.Timestamp,
                IsRead = m.IsRead
            }).ToList();

            Console.WriteLine($"Mapped {dtos.Count} message DTOs for group {groupId}");
            return dtos;
        }

        public async Task<ChatMessageDto> SendMessage(int userId, int groupId, string content)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var group = await _unitOfWork.Chat.GetGroupAsync(groupId);
            if (group == null)
            {
                throw new Exception("Group not found");
            }

            var chatMessage = new ChatMessage
            {
                GroupId = groupId,
                SenderId = userId,
                Content = content,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.Chat.AddMessageAsync(chatMessage);
            await _unitOfWork.SaveChangesAsync();

            return new ChatMessageDto
            {
                Id = chatMessage.Id,
                GroupId = chatMessage.GroupId,
                SenderId = chatMessage.SenderId,
                SenderName = user.FullName,
                SenderRole = user.Role.ToString(),
                Content = chatMessage.Content,
                Timestamp = chatMessage.Timestamp,
                IsRead = chatMessage.IsRead
            };

            
        }

        public async Task<bool> IsUserAuthorizedForGroup(int userId, int groupId)
        {
            return await _unitOfWork.Chat.IsUserAuthorizedForGroupAsync(userId, groupId);
        }

        public async Task MarkMessagesAsRead(int userId, int groupId)
        {
            await _unitOfWork.Chat.MarkMessagesAsReadAsync(userId, groupId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetUnreadMessagesCount(int userId)
        {
            return await _unitOfWork.Chat.GetUnreadMessagesCountAsync(userId);
        }

        public async Task<List<int>> GetUserGroupIds(int userId)
        {
            return await _unitOfWork.Chat.GetUserGroupIdsAsync(userId);
        }
    }
}
