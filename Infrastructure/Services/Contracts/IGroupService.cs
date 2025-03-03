using backend.DTOs;

namespace backend.Infrastructure.Services.Contracts
{
    public interface IGroupService
    {
        Task<GroupDetailsDto> CreateGroupAsync(string creatorEmail, CreateGroupDto groupDto);
        Task<List<GroupDetailsDto>> GetStudentGroupsAsync(int studentId);
        Task<StudentDetailsDto?> GetStudentByEmailAsync(string email);
        Task<GroupDetailsDto?> GetGroupByIdAsync(int groupId);
    }
}