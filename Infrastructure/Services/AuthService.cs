using backend.Core.Entities;
using backend.Core.Enums;
using backend.Core.Settings;
using backend.DTOs;
using backend.Infrastructure.Repositories.Contracts;
using backend.Infrastructure.Services.Contracts;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace backend.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAdminRepository adminRepository,
            ITeacherRepository teacherRepository,
            IStudentRepository studentRepository,
            IConfiguration configuration)
        {
            _adminRepository = adminRepository;
            _teacherRepository = teacherRepository;
            _studentRepository = studentRepository;
            _configuration = configuration;
        }
        public async Task<string?> AuthenticateAsync(string email, string password)
        {
            // Cast each result to User before using null-coalescing operator
            User? user = (await _adminRepository.GetUserByEmailAsync(email) as User) ??
                         (await _teacherRepository.GetUserByEmailAsync(email) as User) ??
                         (await _studentRepository.GetUserByEmailAsync(email) as User);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            return GenerateJwtToken(user);
        }


        public async Task RegisterAdminAsync(Admin admin)
        {
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);
            await _adminRepository.AddAdminAsync(admin);
        }

        public async Task RegisterTeacherAsync(Teacher teacher)
        {
            teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword(teacher.PasswordHash);
            await _teacherRepository.AddTeacherAsync(teacher);
        }

        public async Task RegisterStudentAsync(Student student)
        {
            student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(student.PasswordHash);
            await _studentRepository.AddStudentAsync(student);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["JwtSettings:Secret"]
                ?? throw new InvalidOperationException("JWT secret not configured");
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var studentInfo = user.Role == UserType.Student ? (user as Student) : null;
            var claims = new List<Claim>
    {new Claim("role", user.Role.ToString()),
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),  // "sub" claim for subject
        new Claim(JwtRegisteredClaimNames.Name, user.FullName),
        new Claim("UserId", user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        new Claim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    };
            // Add student-specific claims only if user is a student
            if (studentInfo != null)
            {
                claims.Add(new Claim("department", studentInfo.Department));
                claims.Add(new Claim("enrollmentNumber", studentInfo.EnrollmentNumber));
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                   new SymmetricSecurityKey(key),
                   SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }



        public async Task<List<TeacherDetailsDto>> GetAllTeachersAsync()
        {
            var teachers = await _teacherRepository.GetAllTeachersAsync();
            return teachers.Select(t => new TeacherDetailsDto
            {
                fullName = t.FullName,
                email = t.Email,
                qualification = t.Qualification,
                areaOfSpecialization = t.AreaOfSpecialization,
                officeLocation = t.OfficeLocation
            }).ToList();
        }
    }
}
