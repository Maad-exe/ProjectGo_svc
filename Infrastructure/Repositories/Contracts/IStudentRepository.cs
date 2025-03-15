using backend.Core.Entities;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IStudentRepository
    {
        Task<Student?> GetUserByEmailAsync(string email);
        Task AddStudentAsync(Student student);
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task<bool> ExistsByEnrollmentNumberAsync(string enrollmentNumber);
        Task<bool> ExistsByEnrollmentNumberExceptAsync(string enrollmentNumber, int studentId);
    
}
}
