// Add to DTOs/AdminDashboardDto.cs
namespace backend.DTOs.PanelManagementDTOs
{
    public class AdminDashboardDto
    {
        public int TotalPanels { get; set; }
        public int TotalEvents { get; set; }
        public int TotalGroups { get; set; }
        public int SupervisedGroups { get; set; }
        public int CompletedEvaluations { get; set; }
        public int PendingEvaluations { get; set; }
        public List<EventStatisticsDto> EventStatistics { get; set; } = new List<EventStatisticsDto>();
    }

    public class EventStatisticsDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int TotalGroups { get; set; }
        public int EvaluatedGroups { get; set; }
        public double AveragePerformance { get; set; }
        public DateTime Date { get; set; }
    }
}
