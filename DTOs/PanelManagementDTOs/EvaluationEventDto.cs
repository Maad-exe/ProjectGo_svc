using backend.Core.Enums;

namespace backend.DTOs.PanelManagementDTOs
{
    public class EvaluationEventDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TotalMarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Weight { get; set; } = 1.0;
        public EventType Type { get; set; } = EventType.Final;
        public int? RubricId { get; set; }
        public string? RubricName { get; set; }
    }

    public class CreateEventDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TotalMarks { get; set; }
        public bool IsActive { get; set; }
        public double Weight { get; set; } = 1.0;
        public EventType Type { get; set; } = EventType.Final;
        public int? RubricId { get; set; }
    }

    public class UpdateEventDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TotalMarks { get; set; }
        public bool IsActive { get; set; }
        public EventType Type { get; set; } = EventType.Final;
        public double Weight { get; set; }
        public int? RubricId { get; set; }
    }

    public class GroupEvaluationDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int PanelId { get; set; }
        public string PanelName { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Comments { get; set; } = string.Empty;
        public List<StudentEvaluationDto> StudentEvaluations { get; set; } = new List<StudentEvaluationDto>();
    }

    public class AssignPanelDto
    {
        public int GroupId { get; set; }
        public int PanelId { get; set; }
        public int EventId { get; set; }
        public DateTime ScheduledDate { get; set; }
    }

    public class StudentEvaluationDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ObtainedMarks { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public DateTime EvaluatedAt { get; set; }

        // Additional properties for detailed views
        public string? EventName { get; set; }
        public DateTime? EventDate { get; set; }
        public int? TotalMarks { get; set; }
        public decimal? PercentageObtained { get; set; }
        public bool IsComplete { get; set; }
    }

    public class EvaluateStudentDto
    {
        public int GroupEvaluationId { get; set; }
        public int StudentId { get; set; }

        // For backward compatibility
        public int? ObtainedMarks { get; set; }
        public string? Feedback { get; set; }

        // For rubric-based evaluation
        public List<CategoryScoreDto>? CategoryScores { get; set; }
        
        // Add evaluator ID for tracking who submitted the evaluation
        public int EvaluatorId { get; set; }
    }
    public class CategoryScoreDto
    {
        public int CategoryId { get; set; }
        public int Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }
    public class UpdateGroupEvaluationCommentsDto
    {
        public string Comments { get; set; } = string.Empty;
    }

    // Enhanced Student Evaluation DTO
    public class EnhancedStudentEvaluationDto : StudentEvaluationDto
    {
        public List<CategoryScoreDetailDto> CategoryScores { get; set; } = new List<CategoryScoreDetailDto>();
        public List<EvaluatorDto> Evaluators { get; set; } = new List<EvaluatorDto>();
        public double WeightedScore { get; set; } // Weighted score considering event weight
     // Add this property to fix the error  
    public int? RequiredEvaluatorsCount { get; set; }
    public double? EventWeight { get; set; } // Weight of the event in the overall evaluation

   
    }

    public class CategoryScoreDetailDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public double CategoryWeight { get; set; }
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double WeightedScore { get; set; }
        public string? Feedback { get; set; }
        public EvaluatorDto? Evaluator { get; set; }
        public List<CategoryEvaluatorDetailDto> EvaluatorDetails { get; set; } = new List<CategoryEvaluatorDetailDto>();
    }

    public class EvaluatorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool HasEvaluated { get; set; }
        public int Score { get; set; }
    }

    public class EventEvaluationTypeDto
    {
        public int GroupEvaluationId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public bool HasRubric { get; set; }
        public int? RubricId { get; set; }
        public int TotalMarks { get; set; }
    }

    public class CategoryEvaluatorDetailDto
    {
        public int EvaluatorId { get; set; }
        public string EvaluatorName { get; set; } = string.Empty;
        public int Score { get; set; }
        public string? Feedback { get; set; }
        public DateTime EvaluatedAt { get; set; }
    }

}
