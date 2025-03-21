// DTOs/PanelManagementDTOs/GradeDTOs.cs
namespace backend.DTOs.PanelManagementDTOs
{
    public class NormalizedGradeDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public double RawGrade { get; set; }
        public double NormalizedGrade { get; set; }
    }

    public class StudentFinalGradeDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public double FinalGrade { get; set; }
        public List<EventGradeDto> EventGrades { get; set; } = new List<EventGradeDto>();
    }

    public class EventGradeDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double Score { get; set; } // Percentage
        public double WeightedContribution { get; set; } // Weight * Score
    }
}
