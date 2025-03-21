using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Enums;
using backend.DTOs;
using backend.Infrastructure.Services.Contracts;
using backend.UnitOfWork.Contract;

namespace backend.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ChatMessageDto>> GetGroupMessages(int groupId, int? limit = null, DateTime? before = null, int currentUserId = 0)
        {
            var messages = await _unitOfWork.Chat.GetMessagesForGroupAsync(groupId, limit, before);

            // Get all group members for read status calculation
            var groupMemberIds = await _unitOfWork.Chat.GetGroupMemberIdsAsync(groupId);
            var memberCount = groupMemberIds.Count;

            var dtos = new List<ChatMessageDto>();

            foreach (var m in messages)
            {
                // Count unique users who have read this message
                var readByUserIds = m.ReadStatuses.Select(rs => rs.UserId).ToList();

                // Always consider the sender as having read their own message
                if (!readByUserIds.Contains(m.SenderId))
                {
                    readByUserIds.Add(m.SenderId);
                }

                // Map basic message properties
                var dto = new ChatMessageDto
                {
                    Id = m.Id,
                    GroupId = m.GroupId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender?.FullName ?? "Unknown User",
                    SenderRole = m.Sender?.Role.ToString() ?? "Unknown",
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    IsRead = readByUserIds.Count >= memberCount, // True if all members have read it
                    ReadBy = m.ReadStatuses.Select(rs => new MessageReadStatusDto
                    {
                        UserId = rs.UserId,
                        UserName = rs.User?.FullName ?? "Unknown",
                        ReadAt = rs.ReadAt
                    }).ToList(),
                    TotalReadCount = readByUserIds.Count,
                    IsReadByCurrentUser = readByUserIds.Contains(currentUserId)
                };

                dtos.Add(dto);
            }

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
                Timestamp = DateTime.Now,
                IsRead = false
            };

            await _unitOfWork.Chat.AddMessageAsync(chatMessage);
            await _unitOfWork.SaveChangesAsync();

            // Get all group members for read status calculation
            var groupMemberIds = await _unitOfWork.Chat.GetGroupMemberIdsAsync(groupId);

            return new ChatMessageDto
            {
                Id = chatMessage.Id,
                GroupId = chatMessage.GroupId,
                SenderId = chatMessage.SenderId,
                SenderName = user.FullName,
                SenderRole = user.Role.ToString(),
                Content = chatMessage.Content,
                Timestamp = chatMessage.Timestamp,
                IsRead = chatMessage.IsRead,
                ReadBy = new List<MessageReadStatusDto>(),
                TotalReadCount = 0,
                IsReadByCurrentUser = true // Sender has read their own message
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

        public async Task<Dictionary<int, int>> GetUnreadMessagesByGroup(int userId)
        {
            return await _unitOfWork.Chat.GetUnreadMessagesByGroupAsync(userId);
        }

        public async Task<List<int>> GetUserGroupIds(int userId)
        {
            return await _unitOfWork.Chat.GetUserGroupIdsAsync(userId);
        }
    }
}