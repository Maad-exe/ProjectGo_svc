using backend.Core.Entities;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IAdminRepository
    {
        Task<Admin?> GetUserByEmailAsync(string email);
        Task AddAdminAsync(Admin admin);
    }
}
