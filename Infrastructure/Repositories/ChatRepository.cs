using backend.Core.Entities;
using backend.Core.Enums;
using backend.DTOs;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatMessage>> GetMessagesForGroupAsync(int groupId, int? limit = null, DateTime? before = null)
        {
            IQueryable<ChatMessage> query = _context.ChatMessages
                .Where(m => m.GroupId == groupId)
                .Include(m => m.Sender); // Make sure Sender is always included

            if (before.HasValue)
            {
                query = query.Where(m => m.Timestamp < before.Value);
            }

            // First order by descending to get newest messages if we're applying a limit
            if (limit.HasValue)
            {
                query = query.OrderByDescending(m => m.Timestamp).Take(limit.Value);
                // Then reverse back to ascending order for client display
                return await query.OrderBy(m => m.Timestamp).ToListAsync();
            }
            else
            {
                // If no limit, just return in chronological order
                return await query.OrderBy(m => m.Timestamp).ToListAsync();
            }
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
        
            return message;
        }

        public async Task<User?> GetUserAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<Group?> GetGroupAsync(int groupId)
        {
            return await _context.Groups.FindAsync(groupId);
        }

        public async Task<bool> IsUserAuthorizedForGroupAsync(int userId, int groupId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (user.Role == UserType.Admin)
            {
                return true;
            }

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                return false;
            }

            if (user.Role == UserType.Teacher && group.TeacherId == userId)
            {
                return true;
            }

            if (user.Role == UserType.Student)
            {
                return group.Members.Any(m => m.StudentId == userId);
            }

            return false;
        }

        public async Task MarkMessagesAsReadAsync(int userId, int groupId)
        {
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.GroupId == groupId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            // Don't save changes here - UnitOfWork will do it
        }

        public async Task<int> GetUnreadMessagesCountAsync(int userId)
        {
            var userGroups = await GetUserGroupIdsAsync(userId);

            // Add a semicolon before the WITH clause in the generated SQL
            // This can be done by splitting the query:
            var query = _context.ChatMessages
                .Where(m =>
                    userGroups.Contains(m.GroupId) &&
                    m.SenderId != userId &&
                    !m.IsRead);

            return await query.CountAsync();
        }

        public async Task<List<int>> GetUserGroupIdsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new List<int>();
            }

            if (user.Role == UserType.Student)
            {
                return await _context.GroupMembers
                    .Where(gm => gm.StudentId == userId)
                    .Select(gm => gm.GroupId)
                    .ToListAsync();
            }

            if (user.Role == UserType.Teacher)
            {
                return await _context.Groups
                    .Where(g => g.TeacherId == userId)
                    .Select(g => g.Id)
                    .ToListAsync();
            }

            if (user.Role == UserType.Admin)
            {
                return await _context.Groups
                    .Select(g => g.Id)
                    .ToListAsync();
            }

            return new List<int>();
        }
    }
}
