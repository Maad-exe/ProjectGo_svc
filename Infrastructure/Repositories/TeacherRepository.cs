using backend.Core.Entities;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly AppDbContext _context;

        public TeacherRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Teacher?> GetUserByEmailAsync(string email)
        {
            return await _context.Teachers.SingleOrDefaultAsync(t => t.Email == email);
        }

        public async Task AddTeacherAsync(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Teacher>> GetAllTeachersAsync()
        {
            Console.WriteLine("Attempting to fetch teachers from database...");
            return await _context.Teachers.ToListAsync();
            var teachers = await _context.Teachers.ToListAsync();
            Console.WriteLine($"Found {teachers.Count} teachers");
            return teachers;
        }
    }
}
