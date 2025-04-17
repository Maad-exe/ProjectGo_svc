using backend.Core.Entities;
using backend.DTOs;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IGroupRepository
    {
        Task<Group> CreateGroupAsync(Group group);
        Task<List<Group>> GetStudentGroupsAsync(int studentId);
        Task<Group?> GetGroupByIdAsync(int groupId);
        Task<bool> IsStudentInGroupAsync(int studentId, int groupId);
        Task<bool> UpdateGroupSupervisionRequestAsync(Group group, int teacherId, string message);
        Task<List<SupervisionRequest>> GetSupervisionRequestsForTeacherAsync(int teacherId);
        Task UpdateGroupAsync(Group group);

        Task<SupervisionRequest?> GetSupervisionRequestByGroupIdAndTeacherIdAsync(int groupId, int teacherId);
        Task<IEnumerable<Group>> GetGroupsByTeacherIdAsync(int teacherId);

        Task<(bool InSupervisedGroup, string GroupName, string SupervisorName)> IsStudentInSupervisedGroupAsync(int studentId);
      
        Task<StudentSupervisionStatusDto> GetStudentSupervisionStatusAsync(int studentId);

        Task DeleteSupervisionRequestsForGroupAsync(int groupId);
        Task DeleteGroupAsync(int groupId);
        Task<List<Group>> GetAllGroupsAsync();

       
        Task<IEnumerable<Group>> GetGroupsWithSupervisorsAsync();

    }
}