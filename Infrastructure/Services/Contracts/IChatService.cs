using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs;

namespace backend.Infrastructure.Services.Contracts
{
    public interface IChatService
    {
        Task<List<ChatMessageDto>> GetGroupMessages(int groupId, int? limit = null, DateTime? before = null);
        Task<ChatMessageDto> SendMessage(int userId, int groupId, string content);
        Task<bool> IsUserAuthorizedForGroup(int userId, int groupId);
        Task MarkMessagesAsRead(int userId, int groupId);
        Task<int> GetUnreadMessagesCount(int userId);
        Task<List<int>> GetUserGroupIds(int userId);
    }
}