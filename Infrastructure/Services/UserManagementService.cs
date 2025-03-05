using backend.DTOs;
using backend.Infrastructure.Repositories.Contracts;
using backend.Infrastructure.Services.Contracts;

namespace backend.Infrastructure.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserManagementRepository _userManagementRepository;

        public UserManagementService(IUserManagementRepository userManagementRepository)
        {
            _userManagementRepository = userManagementRepository;
        }

        public async Task<List<UserDetailsDto>> GetAllUsersAsync()
        {
            return await _userManagementRepository.GetAllUsersAsync();
        }

        public async Task<UserDetailsDto?> GetUserByIdAsync(int userId)
        {
            return await _userManagementRepository.GetUserByIdAsync(userId);
        }

        public async Task UpdateUserAsync(UserUpdateDto userUpdateDto)
        {
            await _userManagementRepository.UpdateUserAsync(userUpdateDto);
        }

        public async Task DeleteUserAsync(int userId)
        {
            await _userManagementRepository.DeleteUserAsync(userId);
        }
    }
}