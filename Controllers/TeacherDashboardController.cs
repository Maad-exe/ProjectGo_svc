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
            public async Task<IActionResult> RespondToRequest([FromBody] SupervisionResponseDto response)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int teacherId))
                {
                    return BadRequest("Invalid teacher ID in token");
                }

                _logger.LogInformation($"Teacher {teacherId} responding to supervision request for group {response.GroupId}");

                try
                {
                    var group = await _groupService.RespondToSupervisionRequestAsync(teacherId, response);
                    return Ok(group);
                }
                catch (ApplicationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error responding to supervision request");
                    return StatusCode(500, "An error occurred while responding to the supervision request");
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
