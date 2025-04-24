using backend.Core.Entities;
using backend.Core.Entities.PanelManagement;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class EvaluationRepository : IEvaluationRepository
    {
        private readonly AppDbContext _context;

        public EvaluationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EvaluationEvent> CreateEventAsync(EvaluationEvent evaluationEvent)
        {
            _context.EvaluationEvents.Add(evaluationEvent);
            return evaluationEvent;
        }

        public async Task<EvaluationEvent?> GetEventByIdAsync(int eventId)
        {
            return await _context.EvaluationEvents
                .FirstOrDefaultAsync(e => e.Id == eventId);
        }

        public async Task<List<EvaluationEvent>> GetAllEventsAsync()
        {
            return await _context.EvaluationEvents
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task UpdateEventAsync(EvaluationEvent evaluationEvent)
        {
            _context.EvaluationEvents.Update(evaluationEvent);
        }

        public async Task DeleteEventAsync(int eventId)
        {
            var @event = await _context.EvaluationEvents.FindAsync(eventId);
            if (@event != null)
            {
                _context.EvaluationEvents.Remove(@event);
            }
        }

        public async Task<GroupEvaluation> AssignPanelToGroupAsync(GroupEvaluation groupEvaluation)
        {
            _context.GroupEvaluations.Add(groupEvaluation);
            return groupEvaluation;
        }

        public async Task<List<GroupEvaluation>> GetGroupEvaluationsByPanelIdAsync(int panelId)
        {
            return await _context.GroupEvaluations
                .Where(ge => ge.PanelId == panelId)
                .Include(ge => ge.Group)
                .Include(ge => ge.Panel)
                    .ThenInclude(p => p.Members)
                        .ThenInclude(m => m.Teacher)
                .Include(ge => ge.Event)
                .Include(ge => ge.StudentEvaluations)
                    .ThenInclude(se => se.Student)
                .ToListAsync();
        }

        public async Task<List<GroupEvaluation>> GetGroupEvaluationsByEventIdAsync(int eventId)
        {
            return await _context.GroupEvaluations
                .Where(ge => ge.EventId == eventId)
                .Include(ge => ge.Group)
                .Include(ge => ge.Panel)
                .Include(ge => ge.Event)
                .Include(ge => ge.StudentEvaluations)
                    .ThenInclude(se => se.Student)
                .ToListAsync();
        }

        public async Task<GroupEvaluation?> GetGroupEvaluationByIdAsync(int groupEvaluationId)
        {
            return await _context.GroupEvaluations
                .Include(ge => ge.Group)
                    .ThenInclude(g => g.Members)
                        .ThenInclude(m => m.Student)
                .Include(ge => ge.Panel)
                    .ThenInclude(p => p.Members)
                        .ThenInclude(m => m.Teacher)
                .Include(ge => ge.Event)
                .Include(ge => ge.StudentEvaluations)
                    .ThenInclude(se => se.Student)
                .FirstOrDefaultAsync(ge => ge.Id == groupEvaluationId);
        }

        public async Task<List<GroupEvaluation>> GetGroupEvaluationsByGroupIdAsync(int groupId)
        {
            return await _context.GroupEvaluations
                .Where(ge => ge.GroupId == groupId)
                .Include(ge => ge.Group)
                .Include(ge => ge.Panel)
                .Include(ge => ge.Event)
                .Include(ge => ge.StudentEvaluations)
                    .ThenInclude(se => se.Student)
                .ToListAsync();
        }

        public async Task<StudentEvaluation> EvaluateStudentAsync(StudentEvaluation evaluation)
        {
            _context.StudentEvaluations.Add(evaluation);
            return evaluation;
        }

        public async Task<List<StudentEvaluation>> GetStudentEvaluationsByGroupEvaluationIdAsync(int groupEvaluationId)
        {
            return await _context.StudentEvaluations
                .Where(se => se.GroupEvaluationId == groupEvaluationId)
                .Include(se => se.Student)
                .Include(se => se.GroupEvaluation)
                    .ThenInclude(ge => ge.Event)
                .ToListAsync();
        }

        public async Task<List<StudentEvaluation>> GetStudentEvaluationsByStudentIdAsync(int studentId)
        {
            return await _context.StudentEvaluations
                .Where(se => se.StudentId == studentId)
                .Include(se => se.GroupEvaluation)
                    .ThenInclude(ge => ge.Event)
                .Include(se => se.GroupEvaluation)
                    .ThenInclude(ge => ge.Panel)
                .ToListAsync();
        }

        public async Task UpdateGroupEvaluationAsync(GroupEvaluation groupEvaluation)
        {
            _context.GroupEvaluations.Update(groupEvaluation);
        }
        public async Task<List<GroupEvaluation>> GetAllGroupEvaluationsAsync()
        {
            return await _context.GroupEvaluations
                .Include(ge => ge.Group)
                .Include(ge => ge.Panel)
                .Include(ge => ge.Event)
                .Include(ge => ge.StudentEvaluations)
                .ToListAsync();
        }


        public async Task<bool> HasTeacherEvaluatedStudentAsync(int teacherId, int studentEvaluationId)
        {
            // For rubric-based evaluations
            var categoryScores = await _context.StudentCategoryScores
                .Where(s => s.StudentEvaluationId == studentEvaluationId && s.EvaluatorId == teacherId)
                .ToListAsync();
                
            if (categoryScores.Any())
                return true;
                
            // For simple evaluations (non-rubric)
            var evaluation = await _context.StudentEvaluations
                .Include(se => se.Evaluators)
                .FirstOrDefaultAsync(se => se.Id == studentEvaluationId);
                
            // Check if this teacher is in the evaluators collection
            return evaluation != null && evaluation.Evaluators.Any(e => e.Id == teacherId);
        }

        public async Task<List<Teacher>> GetEvaluatorsByStudentEvaluationIdAsync(int studentEvaluationId)
        {
            var studentEvaluation = await _context.StudentEvaluations
                .Include(se => se.Evaluators)
                .FirstOrDefaultAsync(se => se.Id == studentEvaluationId);

            return studentEvaluation?.Evaluators.ToList() ?? new List<Teacher>();
        }

        public async Task AddEvaluatorToStudentEvaluationAsync(int studentEvaluationId, int teacherId)
        {
            var studentEvaluation = await _context.StudentEvaluations
                .Include(se => se.Evaluators)
                .Include(se => se.GroupEvaluation)
                    .ThenInclude(ge => ge.Panel)
                        .ThenInclude(p => p.Members)
                .FirstOrDefaultAsync(se => se.Id == studentEvaluationId);

            if (studentEvaluation != null)
            {
                var teacher = await _context.Teachers.FindAsync(teacherId);
                if (teacher != null && !studentEvaluation.Evaluators.Any(t => t.Id == teacherId))
                {
                    // Add the evaluator
                    studentEvaluation.Evaluators.Add(teacher);

                    // Set required evaluators count if not set
                    if (studentEvaluation.RequiredEvaluatorsCount == 0)
                    {
                        studentEvaluation.RequiredEvaluatorsCount =
                            studentEvaluation.GroupEvaluation.Panel.Members.Count;
                    }

                    // Update completion status based on actual count
                    studentEvaluation.IsComplete =
                        studentEvaluation.Evaluators.Count >= studentEvaluation.RequiredEvaluatorsCount;

                    await _context.SaveChangesAsync();
                }
            }
        }



        public async Task<bool> CheckEvaluationCompletionAsync(int studentEvaluationId)
        {
            var evaluation = await _context.StudentEvaluations
                .Include(se => se.Evaluators)
                .Include(se => se.GroupEvaluation)
                    .ThenInclude(ge => ge.Panel)
                        .ThenInclude(p => p.Members)
                .FirstOrDefaultAsync(se => se.Id == studentEvaluationId);

            if (evaluation == null) return false;

            int requiredCount = evaluation.GroupEvaluation.Panel.Members.Count;
            int currentCount = evaluation.Evaluators.Count;

            return currentCount >= requiredCount;
        }

        public async Task MarkEvaluationAsCompleteAsync(int evaluationId)
        {
            var evaluation = await _context.StudentEvaluations
                .Include(se => se.Evaluators)
                .Include(se => se.GroupEvaluation)
                    .ThenInclude(ge => ge.Panel)
                        .ThenInclude(p => p.Members)
                .FirstOrDefaultAsync(se => se.Id == evaluationId);

            if (evaluation != null)
            {
                // Only mark complete if all required evaluators have evaluated
                bool isComplete = await CheckEvaluationCompletionAsync(evaluationId);
                evaluation.IsComplete = isComplete;
                _context.StudentEvaluations.Update(evaluation);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<double> CalculateFinalGradeAsync(int studentId)
        {
            // Get all student evaluations grouped by event
            var studentEvaluations = await _context.StudentEvaluations
                .Where(se => se.StudentId == studentId && se.IsComplete)
                .Include(se => se.GroupEvaluation)
                    .ThenInclude(ge => ge.Event)
                .ToListAsync();

            if (!studentEvaluations.Any())
                return 0;

            double weightedSum = 0;
            double totalWeight = 0;

            // Group by event to handle multiple evaluations per event
            var eventGroups = studentEvaluations
                .GroupBy(se => se.GroupEvaluation.EventId);

            foreach (var eventGroup in eventGroups)
            {
                var firstEval = eventGroup.First();
                double eventWeight = firstEval.GroupEvaluation.Event.Weight;

                // Average the marks for this event (if multiple evaluations)
                double eventAverage = eventGroup.Average(se =>
                    (double)se.ObtainedMarks / firstEval.GroupEvaluation.Event.TotalMarks);

                weightedSum += eventAverage * eventWeight;
                totalWeight += eventWeight;
            }

            return totalWeight > 0 ? (weightedSum / totalWeight) * 100 : 0;
        }

        public async Task<List<StudentEvaluation>> GetAllStudentEvaluationsForNormalizationAsync()
        {
            return await _context.StudentEvaluations
                .Where(se => se.IsComplete)
                .Include(se => se.Student)
                .Include(se => se.GroupEvaluation)
                    .ThenInclude(ge => ge.Event)
                .ToListAsync();
        }
        public async Task UpdateStudentEvaluationAsync(StudentEvaluation evaluation)
        {
            _context.StudentEvaluations.Update(evaluation);
        }

        public async Task<StudentEvaluation?> GetStudentEvaluationByIdAsync(int evaluationId)
        {
            return await _context.StudentEvaluations
                .Include(se => se.Student)
                .Include(se => se.GroupEvaluation)
                    .ThenInclude(ge => ge.Event)
                .FirstOrDefaultAsync(se => se.Id == evaluationId);
        }

      
    }
}
