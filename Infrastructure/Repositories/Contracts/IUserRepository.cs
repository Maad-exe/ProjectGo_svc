using backend.Core.Entities;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);

    }
}
