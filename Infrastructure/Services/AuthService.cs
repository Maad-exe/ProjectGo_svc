using backend.Core.Entities;
using backend.Core.Enums;
using backend.Core.Settings;
using backend.DTOs;
using backend.Infrastructure.Data.UnitOfWork.Contract;
using backend.Infrastructure.Repositories;
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
       private readonly IUnitOfWork _unitOfWork;
       private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            // Use UnitOfWork to access repository
            return await _unitOfWork.Users.ExistsByEmailAsync(email);
        }

        public async Task<bool> EnrollmentNumberExistsAsync(string enrollmentNumber)
        {
            // Need to implement in StudentRepository
            return await _unitOfWork.Students.ExistsByEnrollmentNumberAsync(enrollmentNumber);
        }

        public async Task<string?> AuthenticateAsync(string email, string password)
        {
            // Cast each result to User before using null-coalescing operator
            User? user = (await _unitOfWork.Admins.GetUserByEmailAsync(email) as User) ??
                             (await _unitOfWork.Teachers.GetUserByEmailAsync(email) as User) ??
                         (await _unitOfWork.Students.GetUserByEmailAsync(email) as User);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            return GenerateJwtToken(user);
        }


        public async Task RegisterAdminAsync(Admin admin)
        {
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);
            await _unitOfWork.Admins.AddAdminAsync(admin);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RegisterTeacherAsync(Teacher teacher)
        {
            teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword(teacher.PasswordHash);
            await _unitOfWork.Teachers.AddTeacherAsync(teacher);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RegisterStudentAsync(Student student)
        {
            student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(student.PasswordHash);
            await _unitOfWork.Students.AddStudentAsync(student);
            await _unitOfWork.SaveChangesAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT secret not configured");
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var studentInfo = user.Role == UserType.Student ? (user as Student) : null;
            var claims = new List<Claim>
             { new Claim("role", user.Role.ToString()),
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
            var teachers = await _unitOfWork.Teachers.GetAllTeachersAsync();
            return teachers.Select(t => new TeacherDetailsDto
            {  
                Id = t.Id,
                fullName = t.FullName,
                email = t.Email,
                qualification = t.Qualification,
                areaOfSpecialization = t.AreaOfSpecialization,
                officeLocation = t.OfficeLocation,
                AssignedGroups = t.AssignedGroups
            }).ToList();
        }
    }
}
