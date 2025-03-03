using backend.Core.Entities;
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
    }
}