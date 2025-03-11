using backend.Core.Entities;

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

    }
}