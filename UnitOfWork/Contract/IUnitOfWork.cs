using backend.Infrastructure.Repositories.Contracts;
using System;
using System.Threading.Tasks;

namespace backend.UnitOfWork.Contract
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IUserRepository Users { get; }
        IStudentRepository Students { get; }
        ITeacherRepository Teachers { get; }
        IAdminRepository Admins { get; }
        IGroupRepository Groups { get; }
        IUserManagementRepository UserManagement { get; }
        IChatRepository Chat { get; }
        IPanelRepository Panels { get; }
        IEvaluationRepository Evaluations { get; }
        IRubricRepository Rubrics { get; }
        // Save changes
        Task<int> SaveChangesAsync();
    }
}
