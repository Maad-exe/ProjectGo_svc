// Controllers/TeacherController.cs
using backend.Core.Entities;
using backend.DTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/teachers")]
[ApiController]
[Authorize(Policy = "StudentPolicy")]
public class TeacherController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<TeacherController> _logger;

    public TeacherController(IGroupService groupService, ILogger<TeacherController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTeachers()
    {
        _logger.LogInformation("Fetching all teachers");
        try
        {
            var teachers = await _groupService.GetAllTeachersAsync();
            return Ok(teachers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching teachers");
            return StatusCode(500, "An error occurred while fetching teachers");
        }
    }

    [HttpPost("request-supervision")]
    public async Task<IActionResult> RequestSupervision([FromBody] SupervisionRequestDto request)
    {
        _logger.LogInformation($"Requesting supervision for group {request.GroupId} from teacher {request.TeacherId}");
        try
        {
            var result = await _groupService.RequestTeacherSupervisionAsync(request);
            return Ok(new { success = result });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting supervision");
            return StatusCode(500, "An error occurred while requesting supervision");
        }
    }
}

// Controllers/TeacherDashboardController.cs
[Route("api/teacher-dashboard")]
[ApiController]
[Authorize(Policy = "TeacherPolicy")]
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
}
