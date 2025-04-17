// Controllers/AdminRubricController.cs
using backend.DTOs.PanelManagementDTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    //[Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/rubrics")]
    public class AdminRubricController : ControllerBase
    {
        private readonly IEvaluationService _evaluationService;
        private readonly ILogger<AdminRubricController> _logger;

        public AdminRubricController(IEvaluationService evaluationService, ILogger<AdminRubricController> logger)
        {
            _evaluationService = evaluationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<EvaluationRubricDto>> CreateRubric(CreateRubricDto rubricDto)
        {
            try
            {
                var rubric = await _evaluationService.CreateRubricAsync(rubricDto);
                return CreatedAtAction(nameof(GetRubricById), new { id = rubric.Id }, rubric);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EvaluationRubricDto>> GetRubricById(int id)
        {
            var rubric = await _evaluationService.GetRubricByIdAsync(id);
            if (rubric == null)
                return NotFound();

            return rubric;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRubrics()
        {
            try
            {
                var rubrics = await _evaluationService.GetAllRubricsAsync();
                return Ok(rubrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all rubrics");
                return StatusCode(500, new { message = "An error occurred while fetching rubrics" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EvaluationRubricDto>> UpdateRubric(int id, UpdateRubricDto rubricDto)
        {
            try
            {
                var rubric = await _evaluationService.UpdateRubricAsync(id, rubricDto);
                return Ok(rubric);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRubric(int id)
        {
            try
            {
                await _evaluationService.DeleteRubricAsync(id);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("normalized-grades")]
        public async Task<ActionResult<List<NormalizedGradeDto>>> GetNormalizedGrades()
        {
            return await _evaluationService.GetNormalizedGradesAsync();
        }
    }
}
