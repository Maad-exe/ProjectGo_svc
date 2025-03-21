using backend.DTOs.PanelManagementDTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/evaluations")]
    public class AdminEvaluationController : ControllerBase
    {
        private readonly IEvaluationService _evaluationService;
        private readonly IGroupService _groupService;
        private readonly IPanelService _panelService;

        public AdminEvaluationController(
            IEvaluationService evaluationService,
            IGroupService groupService,
            IPanelService panelService)
        {
            _evaluationService = evaluationService;
            _groupService = groupService;
            _panelService = panelService;
        }

        [HttpPost("events")]
        public async Task<ActionResult<EvaluationEventDto>> CreateEvent(CreateEventDto eventDto)
        {
            try
            {
                var evaluationEvent = await _evaluationService.CreateEventAsync(eventDto);
                return CreatedAtAction(nameof(GetEventById), new { id = evaluationEvent.Id }, evaluationEvent);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("events/{id}")]
        public async Task<ActionResult<EvaluationEventDto>> GetEventById(int id)
        {
            var evaluationEvent = await _evaluationService.GetEventByIdAsync(id);
            if (evaluationEvent == null)
                return NotFound();

            return evaluationEvent;
        }

        [HttpGet("events")]
        public async Task<ActionResult<List<EvaluationEventDto>>> GetAllEvents()
        {
            return await _evaluationService.GetAllEventsAsync();
        }

        [HttpPut("events/{id}")]
        public async Task<ActionResult<EvaluationEventDto>> UpdateEvent(int id, UpdateEventDto eventDto)
        {
            try
            {
                var evaluationEvent = await _evaluationService.UpdateEventAsync(id, eventDto);
                return Ok(evaluationEvent);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("events/{id}")]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            try
            {
                await _evaluationService.DeleteEventAsync(id);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("assign-panel")]
        public async Task<ActionResult<GroupEvaluationDto>> AssignPanelToGroup(AssignPanelDto assignDto)
        {
            try
            {
                // First check if panel can evaluate this group
                var group = await _groupService.GetGroupByIdAsync(assignDto.GroupId);
                if (group == null)
                    return NotFound("Group not found");

                if (!group.TeacherId.HasValue)
                    return BadRequest("Cannot assign panel to a group without a supervisor");

                var panel = await _panelService.GetPanelByIdAsync(assignDto.PanelId);
                if (panel == null)
                    return NotFound("Panel not found");

                // Check if any panel member is supervising this group
                foreach (var member in panel.Members)
                {
                    if (member.TeacherId == group.TeacherId)
                        return BadRequest("Cannot assign a panel that includes the group's supervisor");
                }

                var evaluation = await _evaluationService.AssignPanelToGroupAsync(assignDto);
                return CreatedAtAction(nameof(GetGroupEvaluationById), new { id = evaluation.Id }, evaluation);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("group-evaluations/{id}")]
        public async Task<ActionResult<GroupEvaluationDto>> GetGroupEvaluationById(int id)
        {
            var evaluation = await _evaluationService.GetGroupEvaluationByIdAsync(id);
            if (evaluation == null)
                return NotFound();

            return evaluation;
        }

        [HttpGet("events/{eventId}/evaluations")]
        public async Task<ActionResult<List<GroupEvaluationDto>>> GetEvaluationsByEventId(int eventId)
        {
            return await _evaluationService.GetGroupEvaluationsByEventIdAsync(eventId);
        }

        [HttpGet("panels/{panelId}/evaluations")]
        public async Task<ActionResult<List<GroupEvaluationDto>>> GetEvaluationsByPanelId(int panelId)
        {
            return await _evaluationService.GetGroupEvaluationsByPanelIdAsync(panelId);
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard()
        {
            return await _evaluationService.GetAdminDashboardAsync();
        }
    }
}
