using backend.Core.Entities;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetUserByEmailAsync(string email)
        {
            return await _context.Students.SingleOrDefaultAsync(s => s.Email == email);
        }

        public async Task AddStudentAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }
    }
}
