using backend.Core.Entities;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Admin?> GetUserByEmailAsync(string email)
        {
            return await _context.Admins.SingleOrDefaultAsync(a => a.Email == email);
        }

        public async Task AddAdminAsync(Admin admin)
        {
            _context.Admins.Add(admin);
           
        }
    }
}
