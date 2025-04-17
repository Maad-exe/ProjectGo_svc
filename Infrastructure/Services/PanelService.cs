using backend.Core.Entities.PanelManagement;
using backend.DTOs.PanelManagementDTOs;
using backend.Infrastructure.Services.Contracts;
using backend.UnitOfWork.Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Infrastructure.Services
{
    public class PanelService : IPanelService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PanelService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PanelDto> CreatePanelAsync(CreatePanelDto panelDto)
        {
            // Validate panel creation rules
            if (panelDto.TeacherIds.Count < 3)
                throw new ApplicationException("A panel must have at least 3 teachers");

            foreach (var teacherId in panelDto.TeacherIds)
            {
                var teacher = await _unitOfWork.Teachers.GetTeacherByIdAsync(teacherId);
                if (teacher == null)
                    throw new ApplicationException($"Teacher with ID {teacherId} not found");
            }

            // Create the panel
            var panel = new Panel
            {
                Name = panelDto.Name,
                CreatedAt = DateTime.Now
            };

            // Add members to the panel
            for (int i = 0; i < panelDto.TeacherIds.Count; i++)
            {
                panel.Members.Add(new PanelMember
                {
                    TeacherId = panelDto.TeacherIds[i],
                    IsHead = i == 0 // First teacher is the head by default
                });
            }

            var createdPanel = await _unitOfWork.Panels.CreatePanelAsync(panel);
            await _unitOfWork.SaveChangesAsync();

            return await MapPanelToDto(createdPanel);
        }

        public async Task<PanelDto?> GetPanelByIdAsync(int panelId)
        {
            var panel = await _unitOfWork.Panels.GetPanelByIdAsync(panelId);
            if (panel == null)
                return null;

            return await MapPanelToDto(panel);
        }

        public async Task<List<PanelDto>> GetAllPanelsAsync()
        {
            var panels = await _unitOfWork.Panels.GetAllPanelsAsync();
            var result = new List<PanelDto>();

            foreach (var panel in panels)
            {
                result.Add(await MapPanelToDto(panel));
            }

            return result;
        }

        public async Task<PanelDto> UpdatePanelAsync(int panelId, UpdatePanelDto panelDto)
        {
            var panel = await _unitOfWork.Panels.GetPanelByIdAsync(panelId);
            if (panel == null)
                throw new ApplicationException($"Panel with ID {panelId} not found");

            // Validate updated panel
            if (panelDto.TeacherIds.Count < 3)
                throw new ApplicationException("A panel must have at least 3 teachers");

            foreach (var teacherId in panelDto.TeacherIds)
            {
                var teacher = await _unitOfWork.Teachers.GetTeacherByIdAsync(teacherId);
                if (teacher == null)
                    throw new ApplicationException($"Teacher with ID {teacherId} not found");
            }

            // Update panel name
            panel.Name = panelDto.Name;

            // Clear existing members and add new ones
            panel.Members.Clear();
            for (int i = 0; i < panelDto.TeacherIds.Count; i++)
            {
                panel.Members.Add(new PanelMember
                {
                    PanelId = panel.Id,
                    TeacherId = panelDto.TeacherIds[i],
                    IsHead = i == 0 // First teacher is the head by default
                });
            }

            await _unitOfWork.Panels.UpdatePanelAsync(panel);
            await _unitOfWork.SaveChangesAsync();

            return await MapPanelToDto(panel);
        }

        public async Task DeletePanelAsync(int panelId)
        {
            // Check if panel is used in any evaluations
            var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByPanelIdAsync(panelId);
            if (evaluations.Any())
                throw new ApplicationException("Cannot delete panel that is assigned to evaluations");

            await _unitOfWork.Panels.DeletePanelAsync(panelId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<PanelDto>> GetPanelsByTeacherIdAsync(int teacherId)
        {
            var panels = await _unitOfWork.Panels.GetPanelsByTeacherIdAsync(teacherId);
            var result = new List<PanelDto>();

            foreach (var panel in panels)
            {
                result.Add(await MapPanelToDto(panel));
            }

            return result;
        }

        public async Task<bool> CanTeacherEvaluateGroupAsync(int teacherId, int groupId)
        {
            // Get the group
            var group = await _unitOfWork.Groups.GetGroupByIdAsync(groupId);
            if (group == null)
                throw new ApplicationException($"Group with ID {groupId} not found");

            // If group has no supervisor, any teacher can evaluate it
            if (!group.TeacherId.HasValue)
                return true;

            // Teacher cannot evaluate a group they supervise
            return group.TeacherId.Value != teacherId;
        }

        public async Task<List<PanelDto>> GetPanelsByEventIdAsync(int eventId)
        {
            // Verify the event exists
            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(eventId);
            if (evaluationEvent == null)
                throw new ApplicationException($"Event with ID {eventId} not found");

            // Get panels assigned to this event
            var panels = await _unitOfWork.Panels.GetPanelsByEventIdAsync(eventId);
            var result = new List<PanelDto>();

            foreach (var panel in panels)
            {
                result.Add(await MapPanelToDto(panel));
            }

            return result;
        }
        private async Task<PanelDto> MapPanelToDto(Panel panel)
        {
            var dto = new PanelDto
            {
                Id = panel.Id,
                Name = panel.Name,
                CreatedAt = panel.CreatedAt,
                Members = new List<PanelMemberDto>()
            };

            foreach (var member in panel.Members)
            {
                var teacher = await _unitOfWork.Teachers.GetTeacherByIdAsync(member.TeacherId);
                dto.Members.Add(new PanelMemberDto
                {
                    TeacherId = member.TeacherId,
                    TeacherName = teacher?.FullName ?? "Unknown",
                    IsHead = member.IsHead
                });
            }

            return dto;
        }
    }
}
