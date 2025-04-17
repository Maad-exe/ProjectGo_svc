using backend.Core.Entities;
using backend.Core.Enums;
using backend.DTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var token = await _authService.AuthenticateAsync(request.Email, request.Password);
            if (token == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
            return Ok(new { token });
        }

        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto request)
        {
            if (await _authService.EmailExistsAsync(request.Email))
            {
                return BadRequest(new { message = "Email address is already in use" });
            }
            var admin = new Admin
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = request.Password,
                Role = UserType.Admin,
                CreatedAt = DateTime.UtcNow
            };

            await _authService.RegisterAdminAsync(admin);
            return Ok(new { message = "Admin registered successfully" });
        }

        [HttpPost("register/teacher")]
        public async Task<IActionResult> RegisterTeacher([FromBody] RegisterTeacherDto request)
        {
            if (await _authService.EmailExistsAsync(request.Email))
            {
                return BadRequest(new { message = "Email address is already in use" });
            }
            var teacher = new Teacher
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = request.Password,
                Role = UserType.Teacher,
                CreatedAt = DateTime.UtcNow,
                Qualification = request.Qualification,
                AreaOfSpecialization = request.AreaOfSpecialization,
                OfficeLocation = request.OfficeLocation
               
            };

            await _authService.RegisterTeacherAsync(teacher);
            return Ok(new { message = "Teacher registered successfully" });
        }

        [HttpPost("register/student")]
        public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentDto request)
        {

            // Check if email already exists
            if (await _authService.EmailExistsAsync(request.Email))
            {
                return BadRequest(new { message = "Email address is already in use" });
            }

            // Check if enrollment number already exists
            if (await _authService.EnrollmentNumberExistsAsync(request.EnrollmentNumber))
            {
                return BadRequest(new { message = "Enrollment number is already in use" });
            }
            var student = new Student
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = request.Password,
                Role = UserType.Student,
                CreatedAt = DateTime.UtcNow,
                EnrollmentNumber = request.EnrollmentNumber,
                Department = request.Department
            };

            await _authService.RegisterStudentAsync(student);
            return Ok(new { message = "Student registered successfully" });
        }

        [HttpGet("teachers")]
        //[Authorize(Policy = "StudentPolicy")]
        public async Task<IActionResult> GetAllTeachers()
        {
            // Log the current identity and claims before processing
            Console.WriteLine($"User Identity Name: {User.Identity?.Name}");
            Console.WriteLine("User Claims:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }
            Console.WriteLine($"Has Student Role: {User.HasClaim(c => c.Type == "role" && c.Value == "Student")}");

            var teachers = await _authService.GetAllTeachersAsync();
            return Ok(teachers);
        }

    }
}


