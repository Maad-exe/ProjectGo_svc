using backend.Core.Entities;
using backend.DTOs;

namespace backend.Infrastructure.Services.Contracts
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(string email, string password);
        Task RegisterAdminAsync(Admin admin);
        Task RegisterTeacherAsync(Teacher teacher);
        Task RegisterStudentAsync(Student student);
        Task<List<TeacherDetailsDto>> GetAllTeachersAsync();
        Task<bool> EmailExistsAsync(string email);
        Task<bool> EnrollmentNumberExistsAsync(string enrollmentNumber);
        
    }

}
