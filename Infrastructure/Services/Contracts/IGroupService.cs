using backend.DTOs;

namespace backend.Infrastructure.Services.Contracts
{
    public interface IGroupService
    {
        Task<GroupDetailsDto> CreateGroupAsync(string creatorEmail, CreateGroupDto groupDto);
        Task<List<GroupDetailsDto>> GetStudentGroupsAsync(int studentId);
        Task<StudentDetailsDto?> GetStudentByEmailAsync(string email);
        Task<GroupDetailsDto?> GetGroupByIdAsync(int groupId);
        Task<List<TeacherDetailsDto>> GetAllTeachersAsync();
        Task<bool> RequestTeacherSupervisionAsync(SupervisionRequestDto request);
        Task<List<TeacherSupervisionRequestDto>> GetTeacherSupervisionRequestsAsync(int teacherId);
        Task<GroupDetailsDto> RespondToSupervisionRequestAsync(int teacherId, SupervisionResponseDto response);
        Task<IEnumerable<GroupDetailsDto>> GetTeacherGroupsAsync(int teacherId);
        Task<TeacherDetailsDto?> GetTeacherByIdAsync(int teacherId);
    
}
}