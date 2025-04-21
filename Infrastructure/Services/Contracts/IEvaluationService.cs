// Infrastructure/Services/Contracts/IEvaluationService.cs
using backend.Core.Entities.PanelManagement;
using backend.DTOs.PanelManagementDTOs;

public interface IEvaluationService
{
    // Event Management
    Task<EvaluationEventDto> CreateEventAsync(CreateEventDto eventDto);
    Task<EvaluationEventDto?> GetEventByIdAsync(int eventId);
    Task<List<EvaluationEventDto>> GetAllEventsAsync();
    Task<EvaluationEventDto> UpdateEventAsync(int eventId, UpdateEventDto eventDto);
    Task DeleteEventAsync(int eventId);

    // Group Evaluation Management
    Task<GroupEvaluationDto> AssignPanelToGroupAsync(AssignPanelDto assignDto);
    Task<GroupEvaluationDto?> GetGroupEvaluationByIdAsync(int groupEvaluationId);
    Task<List<GroupEvaluationDto>> GetGroupEvaluationsByPanelIdAsync(int panelId);
    Task<List<GroupEvaluationDto>> GetGroupEvaluationsByEventIdAsync(int eventId);
    Task<List<GroupEvaluationDto>> GetGroupEvaluationsByTeacherIdAsync(int teacherId);
    Task<GroupEvaluationDto> UpdateGroupEvaluationAsync(int groupEvaluationId, GroupEvaluation updatedEvaluation);
    Task<GroupEvaluationDto> UpdateGroupEvaluationCommentsAsync(int groupEvaluationId, string comments);

    // Student Evaluation Management
    Task<StudentEvaluationDto> EvaluateStudentAsync(EvaluateStudentDto evaluationDto);
    Task<List<StudentEvaluationDto>> GetStudentEvaluationsByGroupEvaluationIdAsync(int groupEvaluationId);
    Task<List<StudentEvaluationDto>> GetStudentProgressByStudentIdAsync(int studentId);
    // Add this to IEvaluationService.cs
    Task<List<StudentDto>> GetStudentsForGroupEvaluationAsync(int groupEvaluationId, int teacherId);


    // Dashboard & Performance
    Task<List<GroupPerformanceDto>> GetSupervisedGroupsPerformanceAsync(int teacherId);
    Task<TeacherDashboardDto> GetTeacherDashboardAsync(int teacherId);
    Task<AdminDashboardDto> GetAdminDashboardAsync();

    
    Task<EvaluationRubricDto> CreateRubricAsync(CreateRubricDto rubricDto);
    Task<List<EvaluationRubricDto>> GetAllRubricsAsync();
    Task<EvaluationRubricDto?> GetRubricByIdAsync(int rubricId);
    Task<EvaluationRubricDto> UpdateRubricAsync(int rubricId, UpdateRubricDto rubricDto);
    Task DeleteRubricAsync(int rubricId);

    Task<EnhancedStudentEvaluationDto> EvaluateStudentWithRubricAsync(int teacherId, EvaluateStudentDto evaluationDto);
    Task<double> CalculateFinalGradeAsync(int studentId);
    Task<List<NormalizedGradeDto>> GetNormalizedGradesAsync();
    Task<StudentEvaluationDto> GetEvaluationByIdAsync(int evaluationId);
    Task<bool> MarkEvaluationAsCompleteAsync(int evaluationId);

}
