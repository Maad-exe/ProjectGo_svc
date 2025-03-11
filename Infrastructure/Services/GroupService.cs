using backend.Core.Entities;
using backend.Core.Enums;
using backend.DTOs;
using backend.Infrastructure.Repositories.Contracts;
using backend.Infrastructure.Services.Contracts;

namespace backend.Infrastructure.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ITeacherRepository _teacherRepository;


        public GroupService(IGroupRepository groupRepository, IStudentRepository studentRepository, ITeacherRepository teacherRepository)
        {
            _groupRepository = groupRepository;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
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
            var result = new List<GroupDetailsDto>();

            foreach (var group in groups)
            {
                result.Add(await MapGroupToDto(group));
            }

            return result;
        }


        public async Task<GroupDetailsDto?> GetGroupByIdAsync(int groupId)
        {
            var group = await _groupRepository.GetGroupByIdAsync(groupId);
            return group == null ? null : await MapGroupToDto(group);
        }

        

        // Infrastructure/Services/GroupService.cs
        private async Task<GroupDetailsDto> MapGroupToDto(Group group)
        {
            var dto = new GroupDetailsDto
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
                }).ToList(),
                TeacherId = group.TeacherId,
                SupervisionStatus = group.SupervisionStatus.ToString()
            };

            // If a teacher is assigned, get their name
            if (group.TeacherId.HasValue)
            {
                var teacher = await _teacherRepository.GetTeacherByIdAsync(group.TeacherId.Value);
                if (teacher != null)
                {
                    dto.TeacherName = teacher.FullName;
                }
            }

            return dto;
        }


        public async Task<List<TeacherDetailsDto>> GetAllTeachersAsync()
        {
            var teachers = await _teacherRepository.GetAllTeachersAsync();
            return teachers.Select(t => new TeacherDetailsDto
            {
                Id = t.Id,
                fullName = t.FullName,
                email = t.Email,
                qualification = t.Qualification,
                areaOfSpecialization = t.AreaOfSpecialization,
                officeLocation = t.OfficeLocation,
                AssignedGroups = t.AssignedGroups
            }).ToList();
        }

        public async Task<bool> RequestTeacherSupervisionAsync(SupervisionRequestDto request)
        {
            var group = await _groupRepository.GetGroupByIdAsync(request.GroupId);
            if (group == null)
                throw new ApplicationException("Group not found");

            var teacher = await _teacherRepository.GetTeacherByIdAsync(request.TeacherId);
            if (teacher == null)
                throw new ApplicationException("Teacher not found");

            group.SupervisionStatus = GroupSupervisionStatus.Requested;
            return await _groupRepository.UpdateGroupSupervisionRequestAsync(group, request.TeacherId, request.Message);
        }

        public async Task<List<TeacherSupervisionRequestDto>> GetTeacherSupervisionRequestsAsync(int teacherId)
        {
            var requests = await _groupRepository.GetSupervisionRequestsForTeacherAsync(teacherId);

            return requests.Select(r => new TeacherSupervisionRequestDto
            {
                Id = r.Id,
                GroupId = r.Group.Id,
                GroupName = r.Group.Name,
                RequestedAt = r.RequestedAt,
                GroupMembers = r.Group.Members.Select(m => new StudentDetailsDto
                {
                    Id = m.Student.Id,
                    FullName = m.Student.FullName,
                    Email = m.Student.Email,
                    EnrollmentNumber = m.Student.EnrollmentNumber,
                    Department = m.Student.Department,
                    IsCreator = m.IsCreator
                }).ToList(),
                Message = r.Message
            }).ToList();
        }

        public async Task<GroupDetailsDto> RespondToSupervisionRequestAsync(int teacherId, SupervisionResponseDto response)
        {
            var group = await _groupRepository.GetGroupByIdAsync(response.GroupId);
            if (group == null)
                throw new ApplicationException("Group not found");

            // Find the corresponding supervision request
            var supervisionRequest = await _groupRepository.GetSupervisionRequestByGroupIdAndTeacherIdAsync(
                response.GroupId, teacherId);

            if (supervisionRequest == null)
                throw new ApplicationException("Supervision request not found");

            group.SupervisionStatus = response.IsApproved
                ? GroupSupervisionStatus.Approved
                : GroupSupervisionStatus.Rejected;

            if (response.IsApproved)
            {
                group.TeacherId = teacherId;
                await _teacherRepository.IncrementAssignedGroupsAsync(teacherId);
            }

            // Mark the request as processed
            supervisionRequest.IsProcessed = true;

            await _groupRepository.UpdateGroupAsync(group);

            // Return updated group details - add await here
            return await MapGroupToDto(group);
        }


        public async Task<IEnumerable<GroupDetailsDto>> GetTeacherGroupsAsync(int teacherId)
        {
            var groups = await _groupRepository.GetGroupsByTeacherIdAsync(teacherId);
            var result = new List<GroupDetailsDto>();

            // Process each group one by one and await the results
            foreach (var group in groups)
            {
                result.Add(await MapGroupToDto(group));
            }

            return result;
        }


        // Infrastructure/Services/GroupService.cs
        public async Task<TeacherDetailsDto?> GetTeacherByIdAsync(int teacherId)
        {
            var teacher = await _teacherRepository.GetTeacherByIdAsync(teacherId);
            if (teacher == null)
                return null;

            return new TeacherDetailsDto
            {
                Id = teacher.Id,
                fullName = teacher.FullName,
                email = teacher.Email,
                qualification = teacher.Qualification,
                areaOfSpecialization = teacher.AreaOfSpecialization,
                officeLocation = teacher.OfficeLocation,
                AssignedGroups = teacher.AssignedGroups
            };
        }

    }
}