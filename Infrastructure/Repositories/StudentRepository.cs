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

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _context.Students.FindAsync(studentId);
        }

        public async Task AddStudentAsync(Student student)
        {
            _context.Students.Add(student);
        }

        public async Task<bool> ExistsByEnrollmentNumberAsync(string enrollmentNumber)
        {
            return await _context.Students.AnyAsync(s => s.EnrollmentNumber == enrollmentNumber);
        }

        public async Task<bool> ExistsByEnrollmentNumberExceptAsync(string enrollmentNumber, int studentId)
        {
            return await _context.Students.AnyAsync(s => s.EnrollmentNumber == enrollmentNumber && s.Id != studentId);
        }
    }
}
