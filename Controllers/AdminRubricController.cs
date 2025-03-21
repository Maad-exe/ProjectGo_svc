// Controllers/AdminRubricController.cs
using backend.DTOs.PanelManagementDTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/rubrics")]
    public class AdminRubricController : ControllerBase
    {
        private readonly IEvaluationService _evaluationService;

        public AdminRubricController(IEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
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
        public async Task<ActionResult<List<EvaluationRubricDto>>> GetAllRubrics()
        {
            return await _evaluationService.GetAllRubricsAsync();
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
