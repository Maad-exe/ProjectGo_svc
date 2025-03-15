using backend.Infrastructure.Repositories;
using backend.Infrastructure.Repositories.Contracts;
using backend.Infrastructure.Data.UnitOfWork.Contract;

namespace backend.Infrastructure.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IUserRepository _userRepository;
        private IStudentRepository _studentRepository;
        private ITeacherRepository _teacherRepository;
        private IAdminRepository _adminRepository;
        private IGroupRepository _groupRepository;
        private IUserManagementRepository _userManagementRepository;
        private IChatRepository _chatRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IStudentRepository Students => _studentRepository ??= new StudentRepository(_context);
        public ITeacherRepository Teachers => _teacherRepository ??= new TeacherRepository(_context);
        public IAdminRepository Admins => _adminRepository ??= new AdminRepository(_context);
        public IGroupRepository Groups => _groupRepository ??= new GroupRepository(_context);
        public IUserManagementRepository UserManagement => _userManagementRepository ??= new UserManagementRepository(_context);
        public IUserRepository Users => _userRepository ??= new UserRepository(_context);
        public IChatRepository Chat => _chatRepository ??= new ChatRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
