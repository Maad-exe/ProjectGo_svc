// Add to DTOs/GroupPerformanceDto.cs
namespace backend.DTOs.PanelManagementDTOs
{
    public class GroupPerformanceDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public List<EventPerformanceDto> Events { get; set; } = new List<EventPerformanceDto>();
        public double AveragePerformance { get; set; } // Average across all events
        public int CompletedEvents { get; set; }
        public int TotalEvents { get; set; }
    }

    public class EventPerformanceDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public double AverageMarks { get; set; }
        public int TotalMarks { get; set; }
        public double Percentage { get; set; }
        public DateTime EvaluatedOn { get; set; }
        public bool IsCompleted { get; set; }
        public List<StudentPerformanceDto> StudentPerformances { get; set; } = new List<StudentPerformanceDto>();
    }

    public class StudentPerformanceDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ObtainedMarks { get; set; }
        public double Percentage { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }

    public class TeacherDashboardDto
    {
        public int SupervisedGroupCount { get; set; }
        public int PanelMembershipCount { get; set; }
        public int TotalEvaluationsCount { get; set; }
        public List<GroupPerformanceDto> SupervisedGroups { get; set; } = new List<GroupPerformanceDto>();
    }
}
