using backend.Core.Entities;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IGroupRepository
    {
        Task<Group> CreateGroupAsync(Group group);
        Task<List<Group>> GetStudentGroupsAsync(int studentId);
        Task<Group?> GetGroupByIdAsync(int groupId);
        Task<bool> IsStudentInGroupAsync(int studentId, int groupId);
    }
}