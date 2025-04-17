using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using backend.UnitOfWork.Contract;
using System;
using System.Threading.Tasks;

namespace backend.UnitOfWork
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
        private IPanelRepository _panelRepository;
        private IEvaluationRepository _evaluationRepository;
        private IRubricRepository _rubricRepository;
        public UnitOfWork(
    AppDbContext context,
    IUserRepository userRepository,
    IStudentRepository studentRepository,
    ITeacherRepository teacherRepository,
    IAdminRepository adminRepository,
    IGroupRepository groupRepository,
    IUserManagementRepository userManagementRepository,
    IChatRepository chatRepository,
    IPanelRepository panelRepository,
    IEvaluationRepository evaluationRepository,
    IRubricRepository rubricRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _adminRepository = adminRepository;
            _groupRepository = groupRepository;
            _userManagementRepository = userManagementRepository;
            _chatRepository = chatRepository;
            _panelRepository = panelRepository;
            _evaluationRepository = evaluationRepository;
            _rubricRepository = rubricRepository;  // Add this line
        }

        public IUserRepository Users => _userRepository;
        public IStudentRepository Students => _studentRepository;
        public ITeacherRepository Teachers => _teacherRepository;
        public IAdminRepository Admins => _adminRepository;
        public IGroupRepository Groups => _groupRepository;
        public IUserManagementRepository UserManagement => _userManagementRepository;
        public IChatRepository Chat => _chatRepository;
        public IPanelRepository Panels => _panelRepository;
        public IEvaluationRepository Evaluations => _evaluationRepository;
        public IRubricRepository Rubrics => _rubricRepository;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
