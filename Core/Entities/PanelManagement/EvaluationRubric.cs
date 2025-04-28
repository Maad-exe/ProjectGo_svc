// Core/Entities/PanelManagement/EvaluationRubric.cs
namespace backend.Core.Entities.PanelManagement
{
    public class EvaluationRubric
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<RubricCategory> Categories { get; set; } = new List<RubricCategory>();
    }

    public class RubricCategory
    {
        public int Id { get; set; }
        public int RubricId { get; set; }
        public EvaluationRubric Rubric { get; set; } = null!;
        public string Name { get; set; } = string.Empty; // e.g. "Presentation", "Technical Accuracy"
        public string Description { get; set; } = string.Empty;
        public double Weight { get; set; } // e.g. 0.20 for 20%
        public int MaxScore { get; set; } = 10; // Default scale is 0-10
    }

    public class StudentCategoryScore
    {
        public int Id { get; set; }
        public int StudentEvaluationId { get; set; }
        public StudentEvaluation StudentEvaluation { get; set; } = null!;
        public int? CategoryId { get; set; } // Make nullable
        public RubricCategory? Category { get; set; } // Make nullable
        public int Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public int EvaluatorId { get; set; } // Teacher ID who evaluated this category
        public DateTime EvaluatedAt { get; set; } = DateTime.Now;
    }
}
