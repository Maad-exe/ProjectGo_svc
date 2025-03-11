// Controllers/TeacherController.cs
using backend.Core.Entities;
using backend.DTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/teachers")]
[ApiController]
//[Authorize(Policy = "StudentPolicy")]
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

    // Controllers/TeacherController.cs
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeacherById(int id)
    {
        _logger.LogInformation($"Fetching teacher with ID: {id}");
        try
        {
            var teacher = await _groupService.GetTeacherByIdAsync(id);
            if (teacher == null)
                return NotFound(new { message = $"Teacher with ID {id} not found" });

            return Ok(teacher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching teacher with ID {id}");
            return StatusCode(500, "An error occurred while fetching the teacher");
        }
    }

}

