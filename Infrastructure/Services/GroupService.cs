using backend.Core.Entities;
using backend.Core.Enums;
using backend.DTOs;
using backend.Infrastructure.Repositories.Contracts;
using backend.Infrastructure.Services.Contracts;
using backend.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GroupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<StudentDetailsDto?> GetStudentByEmailAsync(string email)
        {
            var student = await _unitOfWork.Students.GetUserByEmailAsync(email);
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
            var creatorStudent = await _unitOfWork.Students.GetUserByEmailAsync(creatorEmail);
            if (creatorStudent == null)
                throw new ApplicationException("Creator student not found");

            // Check if creator is already in a supervised group
            var creatorSupervisedGroupCheck = await _unitOfWork.Groups.IsStudentInSupervisedGroupAsync(creatorStudent.Id);
            if (creatorSupervisedGroupCheck.InSupervisedGroup)
            {
                throw new ApplicationException($"You are already a member of supervised group '{creatorSupervisedGroupCheck.GroupName}' with supervisor {creatorSupervisedGroupCheck.SupervisorName} and cannot create or join another group.");
            }

            var memberStudents = new List<Student> { creatorStudent };

            foreach (var email in groupDto.MemberEmails)
            {
                var student = await _unitOfWork.Students.GetUserByEmailAsync(email);
                if (student == null)
                    throw new ApplicationException($"Student with email {email} not found");

                // Check if this student is already in a supervised group
                var memberSupervisedGroupCheck = await _unitOfWork.Groups.IsStudentInSupervisedGroupAsync(student.Id);
                if (memberSupervisedGroupCheck.InSupervisedGroup)
                {
                    throw new ApplicationException($"Student {student.FullName} is already a member of supervised group '{memberSupervisedGroupCheck.GroupName}' with supervisor {memberSupervisedGroupCheck.SupervisorName} and cannot join another group.");
                }

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

            var createdGroup = await _unitOfWork.Groups.CreateGroupAsync(group);
            await _unitOfWork.SaveChangesAsync(); // Save changes here

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
            var groups = await _unitOfWork.Groups.GetStudentGroupsAsync(studentId);
            var result = new List<GroupDetailsDto>();

            foreach (var group in groups)
            {
                result.Add(await MapGroupToDto(group));
            }

            return result;
        }

        public async Task<GroupDetailsDto?> GetGroupByIdAsync(int groupId)
        {
            var group = await _unitOfWork.Groups.GetGroupByIdAsync(groupId);
            return group == null ? null : await MapGroupToDto(group);
        }

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
                var teacher = await _unitOfWork.Teachers.GetTeacherByIdAsync(group.TeacherId.Value);
                if (teacher != null)
                {
                    dto.TeacherName = teacher.FullName;
                }
            }

            return dto;
        }

        public async Task<List<TeacherDetailsDto>> GetAllTeachersAsync()
        {
            var teachers = await _unitOfWork.Teachers.GetAllTeachersAsync();
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
            var group = await _unitOfWork.Groups.GetGroupByIdAsync(request.GroupId);
            if (group == null)
                throw new ApplicationException("Group not found");

            var teacher = await _unitOfWork.Teachers.GetTeacherByIdAsync(request.TeacherId);
            if (teacher == null)
                throw new ApplicationException("Teacher not found");

            group.SupervisionStatus = GroupSupervisionStatus.Requested;
            var result = await _unitOfWork.Groups.UpdateGroupSupervisionRequestAsync(group, request.TeacherId, request.Message);
            await _unitOfWork.SaveChangesAsync(); // Save changes here
            return result;
        }

        public async Task<List<TeacherSupervisionRequestDto>> GetTeacherSupervisionRequestsAsync(int teacherId)
        {
            var requests = await _unitOfWork.Groups.GetSupervisionRequestsForTeacherAsync(teacherId);

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
            var group = await _unitOfWork.Groups.GetGroupByIdAsync(response.GroupId);
            if (group == null)
                throw new ApplicationException("Group not found");

            // Find the corresponding supervision request
            var supervisionRequest = await _unitOfWork.Groups.GetSupervisionRequestByGroupIdAndTeacherIdAsync(
                response.GroupId, teacherId);

            if (supervisionRequest == null)
                throw new ApplicationException("Supervision request not found");

            group.SupervisionStatus = response.IsApproved
                ? GroupSupervisionStatus.Approved
                : GroupSupervisionStatus.Rejected;

            if (response.IsApproved)
            {
                group.TeacherId = teacherId;
                await _unitOfWork.Teachers.IncrementAssignedGroupsAsync(teacherId);
            }

            // Mark the request as processed
            supervisionRequest.IsProcessed = true;

            await _unitOfWork.Groups.UpdateGroupAsync(group);

            // Save all changes as a single transaction
            await _unitOfWork.SaveChangesAsync();

            // Return updated group details
            return await MapGroupToDto(group);
        }

        public async Task<IEnumerable<GroupDetailsDto>> GetTeacherGroupsAsync(int teacherId)
        {
            var groups = await _unitOfWork.Groups.GetGroupsByTeacherIdAsync(teacherId);
            var result = new List<GroupDetailsDto>();

            // Process each group one by one and await the results
            foreach (var group in groups)
            {
                result.Add(await MapGroupToDto(group));
            }

            return result;
        }

        public async Task<TeacherDetailsDto?> GetTeacherByIdAsync(int teacherId)
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherByIdAsync(teacherId);
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


       

        public async Task CleanupOtherGroupsAsync(int acceptedGroupId)
        {
            try
            {
                // Get the group that was accepted
                var acceptedGroup = await _unitOfWork.Groups.GetGroupByIdAsync(acceptedGroupId);

                if (acceptedGroup == null)
                    throw new ApplicationException("Accepted group not found");

                // Get all student IDs from the accepted group
                var studentIds = acceptedGroup.Members.Select(m => m.StudentId).ToList();

                // Find all other groups that these students are part of (except the accepted one)
                var groupsToDelete = new List<Group>();

                foreach (var studentId in studentIds)
                {
                    var studentGroups = await _unitOfWork.Groups.GetStudentGroupsAsync(studentId);
                    var otherGroups = studentGroups.Where(g => g.Id != acceptedGroupId).ToList();
                    groupsToDelete.AddRange(otherGroups);
                }

                // Make sure we have unique groups
                groupsToDelete = groupsToDelete.DistinctBy(g => g.Id).ToList();

                // Delete all these groups and their related entities
                foreach (var group in groupsToDelete)
                {
                    // Delete all supervision requests for this group
                    await _unitOfWork.Groups.DeleteSupervisionRequestsForGroupAsync(group.Id);

                    // Delete the group (this should cascade delete group members)
                    await _unitOfWork.Groups.DeleteGroupAsync(group.Id);
                }

                // Save all changes together
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to clean up other groups: {ex.Message}", ex);
            }
        }

        public async Task<StudentDetailsDto?> GetStudentByIdAsync(int studentId)
        {
            // First, try to find the student
            var student = await _unitOfWork.Students.GetStudentByIdAsync(studentId);
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

        

        public async Task<StudentSupervisionStatusDto> GetStudentSupervisionStatusAsync(int studentId)
        {
            var supervisionStatus = await _unitOfWork.Groups.IsStudentInSupervisedGroupAsync(studentId);

            return new StudentSupervisionStatusDto
            {
                IsInSupervisedGroup = supervisionStatus.InSupervisedGroup,
                GroupName = supervisionStatus.GroupName,
                SupervisorName = supervisionStatus.SupervisorName
            };
        }

    }
}
