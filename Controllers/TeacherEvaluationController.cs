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
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");
            var userId = int.Parse(userIdClaim);
            return await _evaluationService.GetGroupEvaluationsByTeacherIdAsync(userId);
        }

        [HttpGet("panels")]
        public async Task<ActionResult<List<PanelDto>>> GetTeacherPanels()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");
            var userId = int.Parse(userIdClaim);
            return await _panelService.GetPanelsByTeacherIdAsync(userId);
        }

        [HttpPost("evaluate-student")]
        public async Task<ActionResult<StudentEvaluationDto>> EvaluateStudent([FromBody] EvaluateStudentDto evaluationDto)
        {
            try
            {
                var userIdClaim = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User ID not found in token");
                var userId = int.Parse(userIdClaim);

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
                var userIdClaim = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User ID not found in token");
                var userId = int.Parse(userIdClaim);

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
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");
            var userId = int.Parse(userIdClaim);
            return await _evaluationService.GetSupervisedGroupsPerformanceAsync(userId);
        }

        [HttpPut("group-evaluations/{id}/comments")]
        public async Task<ActionResult<GroupEvaluationDto>> UpdateGroupEvaluationComments(int id, UpdateGroupEvaluationCommentsDto commentsDto)
        {
            try
            {
                var userIdClaim = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User ID not found in token");
                var userId = int.Parse(userIdClaim);

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
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");
            var userId = int.Parse(userIdClaim);
            return await _evaluationService.GetTeacherDashboardAsync(userId);
        }


        

        [HttpPost("evaluate-student-with-rubric")]
        public async Task<ActionResult<EnhancedStudentEvaluationDto>> EvaluateStudentWithRubric([FromBody] EvaluateStudentDto evaluationDto)
        {
            try
            {
                var userIdClaim = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User ID not found in token");
                var userId = int.Parse(userIdClaim);

                // Add logging to debug the issue
                Console.WriteLine($"Processing evaluation from teacher {userId} for student {evaluationDto.StudentId}");

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
                Console.WriteLine($"Error in EvaluateStudentWithRubric: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("group-evaluations/{id}/students")]
        public async Task<ActionResult<List<StudentDto>>> GetStudentsForGroupEvaluation(int id)
        {
            try
            {
                var userIdClaim = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User ID not found in token");
                var userId = int.Parse(userIdClaim);

                var evaluation = await _evaluationService.GetGroupEvaluationByIdAsync(id);
                if (evaluation == null)
                    return NotFound("Group evaluation not found");

                // Verify teacher is in the panel of this evaluation
                var teacherPanels = await _panelService.GetPanelsByTeacherIdAsync(userId);
                var panelIds = teacherPanels.Select(p => p.Id).ToList();

                if (!panelIds.Contains(evaluation.PanelId))
                    return Forbid("You are not a member of the panel for this evaluation");

                // Get students from the group - Pass both groupEvaluationId and teacherId
                var students = await _evaluationService.GetStudentsForGroupEvaluationAsync(id, userId);
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
                var userIdClaim = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User ID not found in token");
                var userId = int.Parse(userIdClaim);

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
                var userIdClaim = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User ID not found in token");
                var userId = int.Parse(userIdClaim);

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

        [HttpGet("evaluation-statistics/{groupEvaluationId}/{studentId}")]
        public async Task<ActionResult<EvaluationStatisticsDto>> GetEvaluationStatistics(int groupEvaluationId, int studentId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User ID not found in token");
                var userId = int.Parse(userIdClaim);
                
                // Verify teacher is in the panel of this evaluation
                var groupEvaluation = await _evaluationService.GetGroupEvaluationByIdAsync(groupEvaluationId);
                if (groupEvaluation == null)
                    return NotFound("Group evaluation not found");
                    
                var teacherPanels = await _panelService.GetPanelsByTeacherIdAsync(userId);
                var panelIds = teacherPanels.Select(p => p.Id).ToList();
                
                if (!panelIds.Contains(groupEvaluation.PanelId))
                    return Forbid("You are not a member of the panel for this evaluation");
                    
                var statistics = await _evaluationService.GetEvaluationStatisticsAsync(groupEvaluationId, studentId);
                return Ok(statistics);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("evaluate-student-with-rubric/{groupEvaluationId}/{studentId}")]
        public async Task<ActionResult<EnhancedStudentEvaluationDto>> GetTeacherEvaluation(int groupEvaluationId, int studentId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User ID not found in token");
                var userId = int.Parse(userIdClaim);
                
                // Add debug logging
                Console.WriteLine($"Teacher {userId} requesting evaluation for student {studentId} in group evaluation {groupEvaluationId}");
                
                // Get the teacher's evaluation specifically
                var teacherEvaluation = await _evaluationService.GetTeacherEvaluationForStudentAsync(
                    userId, groupEvaluationId, studentId);
                
                // More logging
                Console.WriteLine($"Returning evaluation with {teacherEvaluation.CategoryScores?.Count ?? 0} category scores");
                
                return Ok(teacherEvaluation);
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine($"Error in GetTeacherEvaluation: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
    }
}
