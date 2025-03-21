using backend.DTOs;
using backend.Infrastructure.Repositories;
using backend.Infrastructure.Repositories.Contracts;
using backend.Infrastructure.Services.Contracts;
using backend.UnitOfWork.Contract;

namespace backend.Infrastructure.Services
{
    public class UserManagementService : IUserManagementService
    {

        private readonly IUnitOfWork _unitOfWork;
       
        public UserManagementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> EmailExistsExceptUserAsync(string email, int userId)
        {
            // Use UnitOfWork instead of direct repository reference
            return await _unitOfWork.UserManagement.EmailExistsExceptUserAsync(email, userId);
        }

        public async Task<bool> EnrollmentNumberExistsExceptStudentAsync(string enrollmentNumber, int studentId)
        {
            // Use UnitOfWork instead of direct repository reference
            return await _unitOfWork.UserManagement.EnrollmentNumberExistsExceptStudentAsync(enrollmentNumber, studentId);
        }

        public async Task<List<UserDetailsDto>> GetAllUsersAsync()
        {
            return await _unitOfWork.UserManagement.GetAllUsersAsync();
        }

        public async Task<UserDetailsDto?> GetUserByIdAsync(int userId)
        {
            return await _unitOfWork.UserManagement.GetUserByIdAsync(userId);
        }

        public async Task UpdateUserAsync(UserUpdateDto userUpdateDto)
        {
            await _unitOfWork.UserManagement.UpdateUserAsync(userUpdateDto);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            await _unitOfWork.UserManagement.DeleteUserAsync(userId);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}