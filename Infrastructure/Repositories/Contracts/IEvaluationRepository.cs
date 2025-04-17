using backend.Core.Entities;
using backend.Core.Entities.PanelManagement;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IEvaluationRepository
    {
        Task<EvaluationEvent> CreateEventAsync(EvaluationEvent evaluationEvent);
        Task<EvaluationEvent?> GetEventByIdAsync(int eventId);
        Task<List<EvaluationEvent>> GetAllEventsAsync();
        Task UpdateEventAsync(EvaluationEvent evaluationEvent);
        Task DeleteEventAsync(int eventId);

        Task<GroupEvaluation> AssignPanelToGroupAsync(GroupEvaluation groupEvaluation);
        Task<List<GroupEvaluation>> GetGroupEvaluationsByPanelIdAsync(int panelId);
        Task<List<GroupEvaluation>> GetGroupEvaluationsByEventIdAsync(int eventId);
        Task<GroupEvaluation?> GetGroupEvaluationByIdAsync(int groupEvaluationId);
        Task<List<GroupEvaluation>> GetGroupEvaluationsByGroupIdAsync(int groupId);
        
        Task<StudentEvaluation> EvaluateStudentAsync(StudentEvaluation evaluation);
        Task<List<StudentEvaluation>> GetStudentEvaluationsByGroupEvaluationIdAsync(int groupEvaluationId);
        Task<List<StudentEvaluation>> GetStudentEvaluationsByStudentIdAsync(int studentId);
        Task UpdateGroupEvaluationAsync(GroupEvaluation groupEvaluation);

       
        Task<List<GroupEvaluation>> GetAllGroupEvaluationsAsync();

        Task<bool> HasTeacherEvaluatedStudentAsync(int teacherId, int studentEvaluationId);
        Task<List<Teacher>> GetEvaluatorsByStudentEvaluationIdAsync(int studentEvaluationId);
        Task AddEvaluatorToStudentEvaluationAsync(int studentEvaluationId, int teacherId);
        Task<double> CalculateFinalGradeAsync(int studentId);
        Task<List<StudentEvaluation>> GetAllStudentEvaluationsForNormalizationAsync();
        Task UpdateStudentEvaluationAsync(StudentEvaluation evaluation);
        Task<StudentEvaluation?> GetStudentEvaluationByIdAsync(int evaluationId);
        Task MarkEvaluationAsCompleteAsync(int evaluationId);
    }
}
