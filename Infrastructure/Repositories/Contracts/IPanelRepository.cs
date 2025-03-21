using backend.Core.Entities.PanelManagement;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IPanelRepository
    {
        Task<Panel> CreatePanelAsync(Panel panel);
        Task<Panel?> GetPanelByIdAsync(int panelId);
        Task<List<Panel>> GetAllPanelsAsync();
        Task UpdatePanelAsync(Panel panel);
        Task DeletePanelAsync(int panelId);
        Task<bool> IsTeacherInPanelAsync(int panelId, int teacherId);
        Task<List<Panel>> GetPanelsByTeacherIdAsync(int teacherId);
    }
}
