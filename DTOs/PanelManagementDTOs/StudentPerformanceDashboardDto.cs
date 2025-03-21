namespace backend.DTOs.PanelManagementDTOs
{
    public class StudentPerformanceDashboardDto
    {
        public int StudentId { get; set; }
        public int TotalEvaluations { get; set; }
        public double AveragePerformance { get; set; }
        public List<EventSummaryDto> EventSummaries { get; set; } = new List<EventSummaryDto>();
        public List<StudentEvaluationDto> DetailedEvaluations { get; set; } = new List<StudentEvaluationDto>();
    }

    public class EventSummaryDto
    {
        public string EventName { get; set; } = string.Empty;
        public int ObtainedMarks { get; set; }
        public int TotalMarks { get; set; }
        public double Percentage { get; set; }
    }
}
