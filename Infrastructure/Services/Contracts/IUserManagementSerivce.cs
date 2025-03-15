using backend.DTOs;

namespace backend.Infrastructure.Services.Contracts
{
    public interface IUserManagementService
    {
        Task<List<UserDetailsDto>> GetAllUsersAsync();
        Task<UserDetailsDto?> GetUserByIdAsync(int userId);
        Task UpdateUserAsync(UserUpdateDto userUpdateDto);
        Task DeleteUserAsync(int userId);
        Task<bool> EmailExistsExceptUserAsync(string email, int userId);
        Task<bool> EnrollmentNumberExistsExceptStudentAsync(string enrollmentNumber, int studentId);

    }
}