using backend.Core.Entities;
using backend.Core.Enums;
using backend.DTOs;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GroupRepository> _logger;
        public GroupRepository(AppDbContext context, ILogger<GroupRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            _context.Groups.Add(group);
         
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

        //
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

        public async Task DeleteSupervisionRequestsForGroupAsync(int groupId)
        {
            var requests = await _context.SupervisionRequests
                .Where(sr => sr.GroupId == groupId)
                .ToListAsync();

            _context.SupervisionRequests.RemoveRange(requests);
        }

        public async Task DeleteGroupAsync(int groupId)
        {
            // First delete group members to avoid any foreign key constraint issues
            var members = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .ToListAsync();

            _context.GroupMembers.RemoveRange(members);

            // Now delete the group itself
            var group = await _context.Groups.FindAsync(groupId);
            if (group != null)
            {
                _context.Groups.Remove(group);
            }
        }

        public async Task<(bool InSupervisedGroup, string GroupName, string SupervisorName)> IsStudentInSupervisedGroupAsync(int studentId)
        {
            try
            {
                // First, let's log the student we're checking
                _logger.LogInformation($"Checking supervision status for student {studentId}");

                var supervisedGroup = await _context.GroupMembers
                    .Where(gm => gm.StudentId == studentId)
                    .Include(gm => gm.Group)
                    .ThenInclude(g => g.Teacher)
                    .Select(gm => new
                    {
                        GroupId = gm.GroupId,
                        StudentId = gm.StudentId,
                        GroupName = gm.Group.Name,
                        SupervisionStatus = gm.Group.SupervisionStatus,
                        TeacherId = gm.Group.TeacherId,
                        TeacherName = gm.Group.Teacher.FullName
                    })
                    .ToListAsync(); // Get all groups for logging purposes

                // Log all groups found for the student
                foreach (var group in supervisedGroup)
                {
                    _logger.LogInformation(
                        "Found group for student {StudentId}: GroupId={GroupId}, Name={GroupName}, Status={Status}, TeacherId={TeacherId}",
                        studentId,
                        group.GroupId,
                        group.GroupName,
                        group.SupervisionStatus,
                        group.TeacherId);
                }

                // Now find the supervised group if it exists
                var approvedGroup = supervisedGroup.FirstOrDefault(g =>
                    g.SupervisionStatus == GroupSupervisionStatus.Approved &&
                    g.TeacherId != null);

                if (approvedGroup != null)
                {
                    _logger.LogInformation(
                        "Student {StudentId} is in supervised group: GroupId={GroupId}, Name={GroupName}, TeacherId={TeacherId}",
                        studentId,
                        approvedGroup.GroupId,
                        approvedGroup.GroupName,
                        approvedGroup.TeacherId);

                    return (
                        InSupervisedGroup: true,
                        GroupName: approvedGroup.GroupName,
                        SupervisorName: approvedGroup.TeacherName ?? "Unknown"
                    );
                }

                _logger.LogInformation("No supervised group found for student {StudentId}", studentId);
                return (InSupervisedGroup: false, GroupName: "", SupervisorName: "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking supervision status for student {StudentId}", studentId);
                throw;
            }
        }



        public async Task<StudentSupervisionStatusDto> GetStudentSupervisionStatusAsync(int studentId)
        {
            var supervisionStatus = await IsStudentInSupervisedGroupAsync(studentId);

            return new StudentSupervisionStatusDto
            {
                IsInSupervisedGroup = supervisionStatus.InSupervisedGroup,
                GroupName = supervisionStatus.GroupName,
                SupervisorName = supervisionStatus.SupervisorName
            };
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            return await _context.Groups
                .Include(g => g.Members)
                .ToListAsync();
        }

        public async Task<IEnumerable<Group>> GetGroupsWithSupervisorsAsync()
        {
            return await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.Student)
                .Include(g => g.Teacher)
                .Where(g => g.TeacherId != null)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

    }
}



