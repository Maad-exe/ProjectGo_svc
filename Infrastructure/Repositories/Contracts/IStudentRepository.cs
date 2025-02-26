using backend.Core.Entities;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IStudentRepository
    {
        Task<Student?> GetUserByEmailAsync(string email);
        Task AddStudentAsync(Student student);
    }
}
