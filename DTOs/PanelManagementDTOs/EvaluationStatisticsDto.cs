public class EvaluationStatisticsDto
{
    public int StudentEvaluationId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int TotalEvaluators { get; set; }
    public int CompletedEvaluators { get; set; }
    public int RemainingEvaluators { get; set; }
    public bool IsComplete { get; set; }
    public int FinalScore { get; set; }
    public List<EvaluatorSummaryDto> EvaluatorSummaries { get; set; } = new();
}

public class EvaluatorSummaryDto
{
    public int EvaluatorId { get; set; }
    public string EvaluatorName { get; set; } = string.Empty;
    public bool HasEvaluated { get; set; }
    public int? AverageScore { get; set; }
    public DateTime? EvaluatedAt { get; set; }
}