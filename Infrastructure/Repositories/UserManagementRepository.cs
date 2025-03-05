using backend.Core.Entities;
using backend.Core.Enums;
using backend.DTOs;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class UserManagementRepository : IUserManagementRepository
    {
        private readonly AppDbContext _context;

        public UserManagementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDetailsDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Select(u => new UserDetailsDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    CreatedAt = u.CreatedAt,
                    AdditionalInfo = GetAdditionalInfoStatic(u) // Use the static method
                })
                .ToListAsync();

            return users;
        }

        public async Task<UserDetailsDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserDetailsDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    CreatedAt = u.CreatedAt,
                    AdditionalInfo = GetAdditionalInfoStatic(u) // Use the static method
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task UpdateUserAsync(UserUpdateDto userUpdateDto)
        {
            var user = await _context.Users.FindAsync(userUpdateDto.Id);
            if (user == null)
                throw new ApplicationException("User not found");

            user.FullName = userUpdateDto.FullName;
            user.Email = userUpdateDto.Email;

            // Handle type-specific updates
            switch (user)
            {
                case Student student when userUpdateDto.AdditionalInfo is Dictionary<string, object> studentInfo:
                    student.Department = studentInfo.TryGetValue("Department", out var dept) ? dept?.ToString() ?? student.Department : student.Department;
                    student.EnrollmentNumber = studentInfo.TryGetValue("EnrollmentNumber", out var enrollmentNo) ? enrollmentNo?.ToString() ?? student.EnrollmentNumber : student.EnrollmentNumber;
                    break;

                case Teacher teacher when userUpdateDto.AdditionalInfo is Dictionary<string, object> teacherInfo:
                    teacher.Qualification = teacherInfo.TryGetValue("Qualification", out var qual) ? qual?.ToString() ?? teacher.Qualification : teacher.Qualification;
                    teacher.AreaOfSpecialization = teacherInfo.TryGetValue("AreaOfSpecialization", out var area) ? area?.ToString() ?? teacher.AreaOfSpecialization : teacher.AreaOfSpecialization;
                    teacher.AssignedGroups = teacherInfo.TryGetValue("AssignedGroups", out var assignedGroups) ? Convert.ToInt32(assignedGroups) : teacher.AssignedGroups;
                    break;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ApplicationException("User not found");

            // Remove associated group memberships if user is a student
            if (user is Student student)
            {
                var groupMemberships = _context.GroupMembers.Where(gm => gm.StudentId == student.Id);
                _context.GroupMembers.RemoveRange(groupMemberships);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        private static object? GetAdditionalInfoStatic(User user)
        {
            if (user is Student student)
            {
                return new
                {
                    Department = student.Department,
                    EnrollmentNumber = student.EnrollmentNumber
                };
            }
            else if (user is Teacher teacher)
            {
                return new
                {
                    Qualification = teacher.Qualification,
                    AreaOfSpecialization = teacher.AreaOfSpecialization,
                    AssignedGroups = teacher.AssignedGroups // Ensure the property name is correct
                };
            }
            return null;
        }
    }
}
