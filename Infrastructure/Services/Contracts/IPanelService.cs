using backend.DTOs.PanelManagementDTOs;

namespace backend.Infrastructure.Services.Contracts
{
    public interface IPanelService
    {
        Task<PanelDto> CreatePanelAsync(CreatePanelDto panelDto);
        Task<PanelDto?> GetPanelByIdAsync(int panelId);
        Task<List<PanelDto>> GetAllPanelsAsync();
        Task<PanelDto> UpdatePanelAsync(int panelId, UpdatePanelDto panelDto);
        Task DeletePanelAsync(int panelId);
        Task<List<PanelDto>> GetPanelsByTeacherIdAsync(int teacherId);
        Task<bool> CanTeacherEvaluateGroupAsync(int teacherId, int groupId);
        Task<List<PanelDto>> GetPanelsByEventIdAsync(int eventId);
    

}
}
