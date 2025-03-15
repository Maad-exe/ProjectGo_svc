using backend.Core.Entities;
using backend.DTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
  
        [Route("api/teacher-dashboard")]
        [ApiController]
       // [Authorize(Policy = "TeacherPolicy")]
        public class TeacherDashboardController : ControllerBase
        {
            private readonly IGroupService _groupService;
            private readonly ILogger<TeacherDashboardController> _logger;

            public TeacherDashboardController(IGroupService groupService, ILogger<TeacherDashboardController> logger)
            {
                _groupService = groupService;
                _logger = logger;
            }

            [HttpGet("supervision-requests")]
            public async Task<IActionResult> GetSupervisionRequests()
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int teacherId))
                {
                    return BadRequest("Invalid teacher ID in token");
                }

                _logger.LogInformation($"Getting supervision requests for teacher {teacherId}");

                try
                {
                    var requests = await _groupService.GetTeacherSupervisionRequestsAsync(teacherId);
                    return Ok(requests);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching supervision requests");
                    return StatusCode(500, "An error occurred while fetching supervision requests");
                }
            }

            [HttpPost("respond-to-request")]
       
        public async Task<IActionResult> RespondToSupervisionRequest([FromBody] SupervisionResponseDto response)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int teacherId))
                {
                    return BadRequest("Invalid teacher ID in token");
                }

                var result = await _groupService.RespondToSupervisionRequestAsync(teacherId, response);

                // If supervision was approved, clean up other groups
                if (response.IsApproved)
                {
                    await _groupService.CleanupOtherGroupsAsync(response.GroupId);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to supervision request");
                return StatusCode(500, "An error occurred while processing your response");
            }
        }


        [HttpGet("my-groups")]
        public async Task<IActionResult> GetTeacherGroups()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int teacherId))
            {
                return BadRequest("Invalid teacher ID in token");
            }

            _logger.LogInformation($"Getting assigned groups for teacher {teacherId}");

            try
            {
                var groups = await _groupService.GetTeacherGroupsAsync(teacherId);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teacher groups");
                return StatusCode(500, "An error occurred while fetching teacher groups");
            }
        }
    }

    }
