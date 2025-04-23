// Infrastructure/Repositories/RubricRepository.cs
using backend.Core.Entities.PanelManagement;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class RubricRepository : IRubricRepository
    {
        private readonly AppDbContext _context;

        public RubricRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EvaluationRubric> CreateRubricAsync(EvaluationRubric rubric)
        {
            await _context.EvaluationRubrics.AddAsync(rubric);
            return rubric;
        }

        public async Task<EvaluationRubric?> GetRubricByIdAsync(int rubricId)
        {
            return await _context.EvaluationRubrics
                .FirstOrDefaultAsync(r => r.Id == rubricId);
        }

        public async Task<List<EvaluationRubric>> GetAllRubricsAsync()
        {
            return await _context.EvaluationRubrics
                .Where(r => r.IsActive)
                .ToListAsync();
        }

        public async Task<EvaluationRubric?> GetRubricWithCategoriesAsync(int rubricId)
        {
            return await _context.EvaluationRubrics
                .Include(r => r.Categories)
                .FirstOrDefaultAsync(r => r.Id == rubricId);
        }

        public async Task UpdateRubricAsync(EvaluationRubric rubric)
        {
            _context.EvaluationRubrics.Update(rubric);
            await Task.CompletedTask; // To satisfy the async contract
        }

        public async Task DeleteRubricAsync(int rubricId)
        {
            var rubric = await _context.EvaluationRubrics.FindAsync(rubricId);
            if (rubric != null)
            {
                _context.EvaluationRubrics.Remove(rubric);
            }
        }

        public async Task<RubricCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.RubricCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<StudentCategoryScore> AddCategoryScoreAsync(StudentCategoryScore score)
        {
            await _context.StudentCategoryScores.AddAsync(score);
            return score;
        }

        public async Task<List<StudentCategoryScore>> GetScoresByStudentEvaluationIdAsync(int studentEvaluationId)
        {
            return await _context.StudentCategoryScores
                .Where(s => s.StudentEvaluationId == studentEvaluationId)
                .Include(s => s.Category)
                .ToListAsync();
        }

        public async Task<List<StudentCategoryScore>> GetScoresByCategoryIdAsync(int categoryId)
        {
            return await _context.StudentCategoryScores
                .Where(s => s.CategoryId == categoryId)
                .Include(s => s.StudentEvaluation)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentCategoryScore>> GetScoresByStudentEvaluationIdAndEvaluatorIdAsync(int studentEvaluationId, int evaluatorId)
        {
            return await _context.StudentCategoryScores
                .Where(s => s.StudentEvaluationId == studentEvaluationId && s.EvaluatorId == evaluatorId)
                .Include(s => s.Category)
                .ToListAsync();
        }

        public async Task<int> GetUniqueEvaluatorsCountForStudentEvaluationAsync(int studentEvaluationId)
        {
            return await _context.StudentCategoryScores
                .Where(s => s.StudentEvaluationId == studentEvaluationId)
                .Select(s => s.EvaluatorId)
                .Distinct()
                .CountAsync();
        }
    }
}
