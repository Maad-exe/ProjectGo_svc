using backend.Core.Entities;
using backend.Core.Enums;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;

        public GroupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task<List<Group>> GetStudentGroupsAsync(int studentId)
        {
            return await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.Student)
                .Where(g => g.Members.Any(m => m.StudentId == studentId))
                .ToListAsync();
        }

        public async Task<Group?> GetGroupByIdAsync(int groupId)
        {
            return await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.Student)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task<bool> IsStudentInGroupAsync(int studentId, int groupId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.StudentId == studentId && gm.GroupId == groupId);
        }

        // Infrastructure/Repositories/GroupRepository.cs
        public async Task<bool> UpdateGroupSupervisionRequestAsync(Group group, int teacherId, string message)
        {
            // Update group status
            group.SupervisionStatus = GroupSupervisionStatus.Requested;
            _context.Groups.Update(group);

            // Create supervision request
            var request = new SupervisionRequest
            {
                GroupId = group.Id,
                TeacherId = teacherId,
                Message = message,
                RequestedAt = DateTime.UtcNow,
                IsProcessed = false
            };

            _context.SupervisionRequests.Add(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SupervisionRequest>> GetSupervisionRequestsForTeacherAsync(int teacherId)
        {
            return await _context.SupervisionRequests
                .Where(r => r.TeacherId == teacherId && !r.IsProcessed)
                .Include(r => r.Group)
                    .ThenInclude(g => g.Members)
                        .ThenInclude(m => m.Student)
                .ToListAsync();
        }

        public async Task UpdateGroupAsync(Group group)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
        }

        public async Task<SupervisionRequest?> GetSupervisionRequestByGroupIdAndTeacherIdAsync(int groupId, int teacherId)
        {
            return await _context.SupervisionRequests
                .FirstOrDefaultAsync(r => r.GroupId == groupId && r.TeacherId == teacherId && !r.IsProcessed);
        }


        public async Task<IEnumerable<Group>> GetGroupsByTeacherIdAsync(int teacherId)
        {
            return await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.Student)
                .Where(g => g.TeacherId == teacherId)
                .ToListAsync();
        }
    }
}