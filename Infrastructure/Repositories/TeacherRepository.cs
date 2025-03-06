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
           
        }

        public async Task<Teacher?> GetTeacherByIdAsync(int teacherId)
        {
            return await _context.Teachers.FindAsync(teacherId);
        }

        public async Task IncrementAssignedGroupsAsync(int teacherId)
        {
            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher != null)
            {
                teacher.AssignedGroups++;
                await _context.SaveChangesAsync();
            }
        }
    }
}
