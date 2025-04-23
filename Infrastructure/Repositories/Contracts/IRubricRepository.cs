using backend.Core.Entities.PanelManagement;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface IRubricRepository
    {
        Task<EvaluationRubric> CreateRubricAsync(EvaluationRubric rubric);
        Task<EvaluationRubric?> GetRubricByIdAsync(int rubricId);
        Task<List<EvaluationRubric>> GetAllRubricsAsync();
        Task<EvaluationRubric?> GetRubricWithCategoriesAsync(int rubricId);
        Task UpdateRubricAsync(EvaluationRubric rubric);
        Task DeleteRubricAsync(int rubricId);
        Task<RubricCategory?> GetCategoryByIdAsync(int categoryId);
        Task<StudentCategoryScore> AddCategoryScoreAsync(StudentCategoryScore score);
        Task<List<StudentCategoryScore>> GetScoresByStudentEvaluationIdAsync(int studentEvaluationId);
        Task<List<StudentCategoryScore>> GetScoresByCategoryIdAsync(int categoryId);
        Task<IEnumerable<StudentCategoryScore>> GetScoresByStudentEvaluationIdAndEvaluatorIdAsync(int studentEvaluationId, int evaluatorId);
        Task<int> GetUniqueEvaluatorsCountForStudentEvaluationAsync(int studentEvaluationId);
    }
}
