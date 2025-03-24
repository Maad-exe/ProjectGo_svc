using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.DTOs;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IChatRepository
    {
        Task<List<ChatMessage>> GetMessagesForGroupAsync(int groupId, int? limit = null, DateTime? before = null);
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task<User?> GetUserAsync(int userId);
        Task<Group?> GetGroupAsync(int groupId);
        Task<bool> IsUserAuthorizedForGroupAsync(int userId, int groupId);
        Task MarkMessagesAsReadAsync(int userId, int groupId);
        Task<int> GetUnreadMessagesCountAsync(int userId);
        Task<List<int>> GetUserGroupIdsAsync(int userId);
        Task<List<int>> GetGroupMemberIdsAsync(int groupId);
        Task<Dictionary<int, int>> GetUnreadMessagesByGroupAsync(int userId);
        Task<List<MessageReadStatusDto>> GetMessageReadStatusAsync(int messageId);
        Task<GroupInfoDto> GetGroupInfoAsync(int groupId);
    }
}
