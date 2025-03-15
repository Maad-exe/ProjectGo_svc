using backend.DTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Policy = "AdminPolicy")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(IUserManagementService userManagementService, ILogger<UserManagementController> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("Fetching all users");

            try
            {
                var users = await _userManagementService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            _logger.LogInformation($"Fetching user by ID: {userId}");

            try
            {
                var user = await _userManagementService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by ID");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserUpdateDto userUpdateDto)
        {
            if (userId != userUpdateDto.Id)
                return BadRequest("User ID mismatch");

            _logger.LogInformation($"Updating user by ID: {userId}");

            try
            {
                // Check if the new email already exists for another user
                if (await _userManagementService.EmailExistsExceptUserAsync(userUpdateDto.Email, userId))
                {
                    return BadRequest(new { message = "Email address is already in use by another user" });
                }

                // If this is a student and enrollment number is being updated, check uniqueness
                if (userUpdateDto.AdditionalInfo != null &&
                    userUpdateDto.AdditionalInfo.TryGetValue("EnrollmentNumber", out var enrollmentNumber) &&
                    enrollmentNumber != null)
                {
                    if (await _userManagementService.EnrollmentNumberExistsExceptStudentAsync(
                        enrollmentNumber.ToString(), userId))
                    {
                        return BadRequest(new { message = "Enrollment number is already in use by another student" });
                    }
                }

                await _userManagementService.UpdateUserAsync(userUpdateDto);
                return Ok(new { message = "User updated successfully" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Application error updating user");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            _logger.LogInformation($"Deleting user by ID: {userId}");

            try
            {
                await _userManagementService.DeleteUserAsync(userId);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Application error deleting user");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
