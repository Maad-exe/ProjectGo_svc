using backend.DTOs.PanelManagementDTOs;
using backend.Infrastructure.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/panels")]
    public class AdminPanelController : ControllerBase
    {
        private readonly IPanelService _panelService;

        public AdminPanelController(IPanelService panelService)
        {
            _panelService = panelService;
        }

        [HttpPost]
        public async Task<ActionResult<PanelDto>> CreatePanel(CreatePanelDto panelDto)
        {
            try
            {
                var panel = await _panelService.CreatePanelAsync(panelDto);
                return CreatedAtAction(nameof(GetPanelById), new { id = panel.Id }, panel);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PanelDto>> GetPanelById(int id)
        {
            var panel = await _panelService.GetPanelByIdAsync(id);
            if (panel == null)
                return NotFound();

            return panel;
        }

        [HttpGet]
        public async Task<ActionResult<List<PanelDto>>> GetAllPanels()
        {
            return await _panelService.GetAllPanelsAsync();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PanelDto>> UpdatePanel(int id, UpdatePanelDto panelDto)
        {
            try
            {
                var panel = await _panelService.UpdatePanelAsync(id, panelDto);
                return Ok(panel);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePanel(int id)
        {
            try
            {
                await _panelService.DeletePanelAsync(id);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
