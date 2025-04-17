
using backend.Core.Enums;

namespace backend.Core.Entities.PanelManagement
{
    public class EvaluationEvent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TotalMarks { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public double Weight { get; set; } = 1.0;
        public EventType Type { get; set; } = EventType.Final;
        public int? RubricId { get; set; }
        public EvaluationRubric? Rubric { get; set; }
    }
}
