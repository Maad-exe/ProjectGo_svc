using backend.Core.Entities;
using backend.Core.Enums;
using backend.DTOs;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                .Include(m => m.Sender)
                .Include(m => m.ReadStatuses)
                    .ThenInclude(rs => rs.User);  // Include user details in read statuses

            if (before.HasValue)
            {
                query = query.Where(m => m.Timestamp < before.Value);
            }

            // If limit is provided, get the most recent messages before the cutoff
            if (limit.HasValue)
            {
                // First get the messages in descending order (newest first)
                var newestMessages = await query
                    .OrderByDescending(m => m.Timestamp)
                    .Take(limit.Value)
                    .ToListAsync();

                // Then return them in ascending order for display
                return newestMessages.OrderBy(m => m.Timestamp).ToList();
            }
            else
            {
                // No limit, just return all in chronological order
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
            return await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);
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
            var now = DateTime.UtcNow;

            // Get unread messages in the group, excluding those sent by the current user
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.GroupId == groupId && m.SenderId != userId)
                .Include(m => m.ReadStatuses)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                // Check if user already has a read status for this message
                var existingStatus = message.ReadStatuses
                    .FirstOrDefault(rs => rs.UserId == userId);

                if (existingStatus == null)
                {
                    // Create a new read status entry
                    _context.MessageReadStatuses.Add(new MessageReadStatus
                    {
                        MessageId = message.Id,
                        UserId = userId,
                        ReadAt = now
                    });
                }
            }

            // Update the legacy IsRead property
            // A message is considered "read" if all members of the group have read it
            var groupMemberIds = await GetGroupMemberIdsAsync(groupId);

            foreach (var message in unreadMessages)
            {
                // Count unique users who have read this message (including the current user who's marking as read)
                var uniqueReaderIds = message.ReadStatuses
                    .Select(rs => rs.UserId)
                    .Append(userId) // Include current user
                    .Distinct()
                    .ToList();

                // Add the sender as someone who has read the message
                uniqueReaderIds = uniqueReaderIds.Append(message.SenderId).Distinct().ToList();

                // Check if all group members have read the message
                bool allMembersRead = groupMemberIds.All(memberId => uniqueReaderIds.Contains(memberId));

                message.IsRead = allMembersRead;
            }
        }

        public async Task<List<int>> GetGroupMemberIdsAsync(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                return new List<int>();
            }

            var memberIds = new List<int>();

            // Add teacher
            if (group.TeacherId.HasValue)
            {
                memberIds.Add(group.TeacherId.Value);
            }

            // Add students
            memberIds.AddRange(group.Members.Select(m => m.StudentId));

            return memberIds;
        }

        public async Task<int> GetUnreadMessagesCountAsync(int userId)
        {
            try
            {
                var userGroups = await GetUserGroupIdsAsync(userId);
                int totalUnreadCount = 0;

                // Process each group individually
                foreach (var groupId in userGroups)
                {
                    // Direct SQL query using string interpolation - avoid OPENJSON
                    var query = $@"
                SELECT COUNT(*) 
                FROM ChatMessages m
                WHERE m.GroupId = {groupId} 
                AND m.SenderId <> {userId}
                AND NOT EXISTS (
                    SELECT 1 
                    FROM MessageReadStatuses rs 
                    WHERE rs.MessageId = m.Id 
                    AND rs.UserId = {userId}
                )";

                    // Execute raw SQL query
                    using (var command = _context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = query;

                        if (command.Connection.State != System.Data.ConnectionState.Open)
                            await command.Connection.OpenAsync();

                        // Execute the query and get the count
                        var result = await command.ExecuteScalarAsync();
                        if (result != DBNull.Value)
                            totalUnreadCount += Convert.ToInt32(result);
                    }
                }

                return totalUnreadCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUnreadMessagesCountAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<int, int>> GetUnreadMessagesByGroupAsync(int userId)
        {
            try
            {
                var userGroups = await GetUserGroupIdsAsync(userId);
                var result = new Dictionary<int, int>();

                // Initialize all groups with zero unread messages
                foreach (var groupId in userGroups)
                {
                    // Direct SQL query
                    var query = $@"
                SELECT COUNT(*) 
                FROM ChatMessages m
                WHERE m.GroupId = {groupId} 
                AND m.SenderId <> {userId}
                AND NOT EXISTS (
                    SELECT 1 
                    FROM MessageReadStatuses rs 
                    WHERE rs.MessageId = m.Id 
                    AND rs.UserId = {userId}
                )";

                    // Execute raw SQL query
                    using (var command = _context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = query;

                        if (command.Connection.State != System.Data.ConnectionState.Open)
                            await command.Connection.OpenAsync();

                        // Execute the query and get the count
                        var queryResult = await command.ExecuteScalarAsync();
                        int unreadCount = 0;
                        if (queryResult != DBNull.Value)
                            unreadCount = Convert.ToInt32(queryResult);

                        result[groupId] = unreadCount;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUnreadMessagesByGroupAsync: {ex.Message}");
                throw;
            }
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

        // New method to get read status details for a message
        public async Task<List<MessageReadStatusDto>> GetMessageReadStatusAsync(int messageId)
        {
            var readStatuses = await _context.MessageReadStatuses
                .Where(rs => rs.MessageId == messageId)
                .Include(rs => rs.User)
                .Select(rs => new MessageReadStatusDto
                {
                    UserId = rs.UserId,
                    UserName = rs.User.FullName,
                    ReadAt = rs.ReadAt
                })
                .ToListAsync();

            return readStatuses;
        }

        // New method to get group info with member count
        public async Task<GroupInfoDto> GetGroupInfoAsync(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .Include(g => g.Teacher)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                return null;
            }

            return new GroupInfoDto
            {
                Id = group.Id,
                Name = group.Name,
                TeacherId = group.TeacherId,
                TeacherName = group.Teacher?.FullName,
                MemberCount = group.Members.Count + (group.TeacherId.HasValue ? 1 : 0)
            };
        }
    }
}