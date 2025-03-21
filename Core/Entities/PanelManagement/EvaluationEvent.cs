namespace backend.Core.Entities.PanelManagement
{
    public class EvaluationEvent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Proposal Defense", "30% Evaluation", etc.
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TotalMarks { get; set; } // Maximum possible marks
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // Add weight for final grade calculation (e.g., 0.3 for 30% of final grade)
        public double Weight { get; set; } = 1.0;

        // Add reference to rubric
        public int? RubricId { get; set; }
        public EvaluationRubric? Rubric { get; set; }
    }

}
