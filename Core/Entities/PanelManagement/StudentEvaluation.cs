// Core/Entities/PanelManagement/StudentEvaluation.cs
using backend.Core.Entities.PanelManagement;
using backend.Core.Entities;

public class StudentEvaluation
{
    public int Id { get; set; }
    public int GroupEvaluationId { get; set; }
    public GroupEvaluation GroupEvaluation { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    // Total calculated score - will be computed from category scores
    public int ObtainedMarks { get; set; }

    // General feedback about the entire evaluation
    public string Feedback { get; set; } = string.Empty;

    // Date when evaluation was completed (all categories scored)
    public DateTime EvaluatedAt { get; set; } = DateTime.Now;

    // Flag to indicate if this evaluation is completed
    public bool IsComplete { get; set; } = false;

    // For multiple evaluators and rubric-based evaluation
    public int? RubricId { get; set; }
    public EvaluationRubric? Rubric { get; set; }
    public int RequiredEvaluatorsCount { get; set; } // Add this property


    // Collection of category scores from different evaluators
    public ICollection<StudentCategoryScore> CategoryScores { get; set; } = new List<StudentCategoryScore>();

    // Collection of evaluators who have evaluated this student
    public ICollection<Teacher> Evaluators { get; set; } = new List<Teacher>();
}
