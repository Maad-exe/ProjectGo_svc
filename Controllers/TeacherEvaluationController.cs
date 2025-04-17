using backend.DTOs.PanelManagementDTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    //[Authorize(Roles = "Teacher")]
    [ApiController]
    [Route("api/teacher/evaluations")]
    public class TeacherEvaluationController : ControllerBase
    {
        private readonly IEvaluationService _evaluationService;
        private readonly IPanelService _panelService;

        public TeacherEvaluationController(
            IEvaluationService evaluationService,
            IPanelService panelService)
        {
            _evaluationService = evaluationService;
            _panelService = panelService;
        }

        [HttpGet("panel-assignments")]
        public async Task<ActionResult<List<GroupEvaluationDto>>> GetTeacherPanelAssignments()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            return await _evaluationService.GetGroupEvaluationsByTeacherIdAsync(userId);
        }

        [HttpGet("panels")]
        public async Task<ActionResult<List<PanelDto>>> GetTeacherPanels()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            return await _panelService.GetPanelsByTeacherIdAsync(userId);
        }

        [HttpPost("evaluate-student")]
        public async Task<ActionResult<StudentEvaluationDto>> EvaluateStudent([FromBody] EvaluateStudentDto evaluationDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));

                // Verify teacher is in the panel of this evaluation
                var groupEvaluation = await _evaluationService.GetGroupEvaluationByIdAsync(evaluationDto.GroupEvaluationId);
                if (groupEvaluation == null)
                    return NotFound("Group evaluation not found");

                var teacherPanels = await _panelService.GetPanelsByTeacherIdAsync(userId);
                var panelIds = teacherPanels.Select(p => p.Id).ToList();

                if (!panelIds.Contains(groupEvaluation.PanelId))
                    return Forbid("You are not a member of the panel for this evaluation");

                // Also check if teacher is not the supervisor of the group
                var canEvaluate = await _panelService.CanTeacherEvaluateGroupAsync(userId, groupEvaluation.GroupId);
                if (!canEvaluate)
                    return BadRequest("You cannot evaluate a group you supervise");

                var evaluation = await _evaluationService.EvaluateStudentAsync(evaluationDto);
                return Ok(evaluation);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("group-evaluations/{id}")]
        public async Task<ActionResult<GroupEvaluationDto>> GetGroupEvaluationById(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));

                var evaluation = await _evaluationService.GetGroupEvaluationByIdAsync(id);
                if (evaluation == null)
                    return NotFound();

                // Verify teacher is in the panel of this evaluation
                var teacherPanels = await _panelService.GetPanelsByTeacherIdAsync(userId);
                var panelIds = teacherPanels.Select(p => p.Id).ToList();

                if (!panelIds.Contains(evaluation.PanelId))
                    return Forbid("You are not a member of the panel for this evaluation");

                return evaluation;
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("supervised-groups/performance")]
        public async Task<ActionResult<List<GroupPerformanceDto>>> GetSupervisedGroupsPerformance()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            return await _evaluationService.GetSupervisedGroupsPerformanceAsync(userId);
        }

        [HttpPut("group-evaluations/{id}/comments")]
        public async Task<ActionResult<GroupEvaluationDto>> UpdateGroupEvaluationComments(int id, UpdateGroupEvaluationCommentsDto commentsDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));

                // Verify teacher is in the panel of this evaluation
                var groupEvaluation = await _evaluationService.GetGroupEvaluationByIdAsync(id);
                if (groupEvaluation == null)
                    return NotFound("Group evaluation not found");

                var teacherPanels = await _panelService.GetPanelsByTeacherIdAsync(userId);
                var panelIds = teacherPanels.Select(p => p.Id).ToList();

                if (!panelIds.Contains(groupEvaluation.PanelId))
                    return Forbid("You are not a member of the panel for this evaluation");

                var updatedEvaluation = await _evaluationService.UpdateGroupEvaluationCommentsAsync(id, commentsDto.Comments);
                return Ok(updatedEvaluation);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<TeacherDashboardDto>> GetTeacherDashboard()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            return await _evaluationService.GetTeacherDashboardAsync(userId);
        }


        

        [HttpPost("evaluate-student-with-rubric")]
        public async Task<ActionResult<EnhancedStudentEvaluationDto>> EvaluateStudentWithRubric(EvaluateStudentDto evaluationDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));

                // Verify teacher is in the panel of this evaluation
                var groupEvaluation = await _evaluationService.GetGroupEvaluationByIdAsync(evaluationDto.GroupEvaluationId);
                if (groupEvaluation == null)
                    return NotFound("Group evaluation not found");

                var teacherPanels = await _panelService.GetPanelsByTeacherIdAsync(userId);
                var panelIds = teacherPanels.Select(p => p.Id).ToList();

                if (!panelIds.Contains(groupEvaluation.PanelId))
                    return Forbid("You are not a member of the panel for this evaluation");

                // Also check if teacher is not the supervisor of the group
                var canEvaluate = await _panelService.CanTeacherEvaluateGroupAsync(userId, groupEvaluation.GroupId);
                if (!canEvaluate)
                    return BadRequest("You cannot evaluate a group you supervise");

                var evaluation = await _evaluationService.EvaluateStudentWithRubricAsync(userId, evaluationDto);
                return Ok(evaluation);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("group-evaluations/{id}/students")]
        public async Task<ActionResult<List<StudentDto>>> GetStudentsForGroupEvaluation(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));

                var evaluation = await _evaluationService.GetGroupEvaluationByIdAsync(id);
                if (evaluation == null)
                    return NotFound("Group evaluation not found");

                // Verify teacher is in the panel of this evaluation
                var teacherPanels = await _panelService.GetPanelsByTeacherIdAsync(userId);
                var panelIds = teacherPanels.Select(p => p.Id).ToList();

                if (!panelIds.Contains(evaluation.PanelId))
                    return Forbid("You are not a member of the panel for this evaluation");

                // Get students from the group
                var students = await _evaluationService.GetStudentsForGroupEvaluationAsync(id);
                return Ok(students);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("event-evaluation-type/{id}")]
        public async Task<ActionResult<EventEvaluationTypeDto>> GetEventEvaluationType(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));

                // Verify the group evaluation exists
                var groupEvaluation = await _evaluationService.GetGroupEvaluationByIdAsync(id);
                if (groupEvaluation == null)
                    return NotFound("Group evaluation not found");

                // Verify teacher is in the panel of this evaluation
                var teacherPanels = await _panelService.GetPanelsByTeacherIdAsync(userId);
                var panelIds = teacherPanels.Select(p => p.Id).ToList();

                if (!panelIds.Contains(groupEvaluation.PanelId))
                    return Forbid("You are not a member of the panel for this evaluation");

                // Get the evaluation event to check if it has a rubric
                var eventDetails = await _evaluationService.GetEventByIdAsync(groupEvaluation.EventId);
                if (eventDetails == null)
                    return NotFound("Evaluation event not found");

                // Return info about the evaluation type
                return Ok(new EventEvaluationTypeDto
                {
                    GroupEvaluationId = id,
                    EventId = eventDetails.Id,
                    EventName = eventDetails.Name,
                    HasRubric = eventDetails.RubricId.HasValue,
                    RubricId = eventDetails.RubricId,
                    TotalMarks = eventDetails.TotalMarks
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("evaluations/{id}/complete")]
        public async Task<ActionResult> CompleteEvaluation(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));

                // Get the evaluation
                var evaluation = await _evaluationService.GetEvaluationByIdAsync(id);
                if (evaluation == null)
                    return NotFound("Evaluation not found");

                // Verify the teacher is authorized (either created the evaluation or is admin)
                //if (evaluation.TeacherId != userId && !User.IsInRole("Admin"))
                //    return Forbid("You are not authorized to complete this evaluation");

                // Mark evaluation as complete
                await _evaluationService.MarkEvaluationAsCompleteAsync(id);

                return Ok(new { message = "Evaluation marked as complete" });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
