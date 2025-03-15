using backend.Infrastructure.Repositories.Contracts;

namespace backend.Infrastructure.Data.UnitOfWork.Contract
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
        // Save changes
        Task<int> SaveChangesAsync();
        
    }
}
