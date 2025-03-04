using backend.Core.Entities;
using backend.DTOs;
using backend.Infrastructure.Repositories.Contracts;
using backend.Infrastructure.Services.Contracts;

namespace backend.Infrastructure.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IStudentRepository _studentRepository;

        public GroupService(IGroupRepository groupRepository, IStudentRepository studentRepository)
        {
            _groupRepository = groupRepository;
            _studentRepository = studentRepository;
        }

        public async Task<StudentDetailsDto?> GetStudentByEmailAsync(string email)
        {
            var student = await _studentRepository.GetUserByEmailAsync(email);
            if (student == null)
                return null;

            return new StudentDetailsDto
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                EnrollmentNumber = student.EnrollmentNumber,
                Department = student.Department
            };
        }

        public async Task<GroupDetailsDto> CreateGroupAsync(string creatorEmail, CreateGroupDto groupDto)
        {
            // Validate that all student emails exist
            var creatorStudent = await _studentRepository.GetUserByEmailAsync(creatorEmail);
            if (creatorStudent == null)
                throw new ApplicationException("Creator student not found");

            var memberStudents = new List<Student> { creatorStudent };

            foreach (var email in groupDto.MemberEmails)
            {
                var student = await _studentRepository.GetUserByEmailAsync(email);
                if (student == null)
                    throw new ApplicationException($"Student with email {email} not found");

                memberStudents.Add(student);
            }

            // Create the group
            var group = new Group
            {
                Name = groupDto.GroupName,
                CreatedAt = DateTime.UtcNow
            };

            // Add creator as first member (and mark as creator)
            group.Members.Add(new GroupMember
            {
                StudentId = creatorStudent.Id,
                IsCreator = true,
                JoinedAt = DateTime.UtcNow
            });

            // Add other members
            foreach (var student in memberStudents.Where(s => s.Id != creatorStudent.Id))
            {
                group.Members.Add(new GroupMember
                {
                    StudentId = student.Id,
                    IsCreator = false,
                    JoinedAt = DateTime.UtcNow
                });
            }

            var createdGroup = await _groupRepository.CreateGroupAsync(group);

            // Convert to DTO
            return new GroupDetailsDto
            {
                Id = createdGroup.Id,
                Name = createdGroup.Name,
                CreatedAt = createdGroup.CreatedAt,
                Members = memberStudents.Select(s => new StudentDetailsDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    EnrollmentNumber = s.EnrollmentNumber,
                    Department = s.Department
                }).ToList()
            };
        }

        public async Task<List<GroupDetailsDto>> GetStudentGroupsAsync(int studentId)
        {
            var groups = await _groupRepository.GetStudentGroupsAsync(studentId);
            return groups.Select(MapGroupToDto).ToList();
        }

        public async Task<GroupDetailsDto?> GetGroupByIdAsync(int groupId)
        {
            var group = await _groupRepository.GetGroupByIdAsync(groupId);
            return group == null ? null : MapGroupToDto(group);
        }

        private GroupDetailsDto MapGroupToDto(Group group)
        {
            return new GroupDetailsDto
            {
                Id = group.Id,
                Name = group.Name,
                CreatedAt = group.CreatedAt,
                Members = group.Members.Select(m => new StudentDetailsDto
                {
                    Id = m.Student.Id,
                    FullName = m.Student.FullName,
                    Email = m.Student.Email,
                    EnrollmentNumber = m.Student.EnrollmentNumber,
                    Department = m.Student.Department,
                    IsCreator = m.IsCreator
                }).ToList()
            };
        }
    }
}