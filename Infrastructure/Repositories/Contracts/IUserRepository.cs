using backend.Core.Entities;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);
        Task AddUserAsync(User user);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByEmailExceptAsync(string email, int userId);
    }

}
