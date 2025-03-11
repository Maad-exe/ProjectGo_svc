using backend.Core.Entities;
using backend.DTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/groups")]
    [ApiController]
    [Authorize(Policy = "StudentPolicy")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentGroups(int studentId)
        {
           
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userIdClaim == null || int.Parse(userIdClaim) != studentId)
            {
                return Forbid();
            }

            var groups = await _groupService.GetStudentGroupsAsync(studentId);
            return Ok(groups);
        }

        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroupById(int groupId)
        {
            var group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
                return NotFound();

            return Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto groupDto)
        {
            try
            {
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("User email not found in token");
                }

                var group = await _groupService.CreateGroupAsync(userEmail, groupDto);
                return CreatedAtAction(nameof(GetGroupById), new { groupId = group.Id }, group);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("student/search")]
        public async Task<IActionResult> GetStudentByEmail([FromQuery] string email)
        {
            var student = await _groupService.GetStudentByEmailAsync(email);
            if (student == null)
                return NotFound();

            return Ok(student);
        }


        // Controllers/GroupController.cs
        [HttpGet("supervision-status")]
        public async Task<IActionResult> GetGroupsSupervisionStatus([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest("Group IDs are required");

            var groupIds = ids.Split(',')
                .Select(id => int.TryParse(id, out int result) ? result : 0)
                .Where(id => id > 0)
                .ToList();

            if (!groupIds.Any())
                return BadRequest("Invalid group IDs provided");

            var resultDict = new Dictionary<string, object>();

            foreach (var groupId in groupIds)
            {
                var group = await _groupService.GetGroupByIdAsync(groupId);
                if (group != null)
                {
                    resultDict.Add(groupId.ToString(), new
                    {
                        status = group.SupervisionStatus,
                        teacherId = group.TeacherId
                    });
                }
            }

            return Ok(resultDict);
        }


    }
}