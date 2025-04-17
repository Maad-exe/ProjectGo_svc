using backend.Core.Entities.PanelManagement;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class PanelRepository : IPanelRepository
    {
        private readonly AppDbContext _context;

        public PanelRepository(AppDbContext context)
        {
            _context = context;
        }

        // Implement the methods from the interface
        public async Task<Panel> CreatePanelAsync(Panel panel)
        {
            _context.Panels.Add(panel);
            return panel;
        }

        public async Task<Panel?> GetPanelByIdAsync(int panelId)
        {
            return await _context.Panels
                .Include(p => p.Members)
                    .ThenInclude(m => m.Teacher)
                .FirstOrDefaultAsync(p => p.Id == panelId);
        }
        public async Task<List<Panel>> GetAllPanelsAsync()
        {
            return await _context.Panels
                .Include(p => p.Members)
                    .ThenInclude(m => m.Teacher)
                .ToListAsync();
        }

        public async Task UpdatePanelAsync(Panel panel)
        {
            _context.Panels.Update(panel);
        }

        public async Task DeletePanelAsync(int panelId)
        {
            var panel = await _context.Panels.FindAsync(panelId);
            if (panel != null)
            {
                _context.Panels.Remove(panel);
            }
        }

        public async Task<bool> IsTeacherInPanelAsync(int panelId, int teacherId)
        {
            return await _context.PanelMembers
                .AnyAsync(pm => pm.PanelId == panelId && pm.TeacherId == teacherId);
        }
        public async Task<List<Panel>> GetPanelsByTeacherIdAsync(int teacherId)
        {
            return await _context.PanelMembers
                .Where(pm => pm.TeacherId == teacherId)
                .Include(pm => pm.Panel)
                .Select(pm => pm.Panel)
                .ToListAsync();
        }

        public async Task<List<Panel>> GetPanelsByEventIdAsync(int eventId)
        {
            // Get panel IDs that are used in group evaluations for this event
            var panelIds = await _context.GroupEvaluations
                .Where(ge => ge.EventId == eventId)
                .Select(ge => ge.PanelId)
                .Distinct()
                .ToListAsync();

            // Fetch the corresponding panels with their members
            return await _context.Panels
                .Where(p => panelIds.Contains(p.Id))
                .Include(p => p.Members)
                    .ThenInclude(m => m.Teacher)
                .ToListAsync();
        }
    }
}

