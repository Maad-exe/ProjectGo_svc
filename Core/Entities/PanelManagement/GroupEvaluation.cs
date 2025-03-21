
namespace backend.Core.Entities.PanelManagement
{
    public class GroupEvaluation
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;
        public int PanelId { get; set; }
        public Panel Panel { get; set; } = null!;
        public int EventId { get; set; }
        public EvaluationEvent Event { get; set; } = null!;
        public DateTime ScheduledDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public string Comments { get; set; } = string.Empty;
        public ICollection<StudentEvaluation> StudentEvaluations { get; set; } = new List<StudentEvaluation>();
    }
}
