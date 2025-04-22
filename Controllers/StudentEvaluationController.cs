using backend.Core.Entities;
using backend.DTOs.PanelManagementDTOs;
using backend.Infrastructure.Services.Contracts;
using backend.UnitOfWork.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.UnitOfWork;

namespace backend.Controllers
{
    //[Authorize(Roles = "Student")]
    [ApiController]
    [Route("api/student/evaluations")]
    public class StudentEvaluationController : ControllerBase
    {
        private readonly IEvaluationService _evaluationService;
        private readonly IUnitOfWork _unitOfWork;
        public StudentEvaluationController(IEvaluationService evaluationService, IUnitOfWork unitOfWork)
        {
            _evaluationService = evaluationService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("progress")]
        public async Task<ActionResult<List<StudentEvaluationDto>>> GetStudentProgress()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            return await _evaluationService.GetStudentProgressByStudentIdAsync(userId);
        }


        // In StudentEvaluationController.cs - GetStudentDashboard method
        [HttpGet("dashboard")]
        public async Task<ActionResult<StudentPerformanceDashboardDto>> GetStudentDashboard()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));

            // Get all evaluations for this student
            var evaluations = await _evaluationService.GetStudentProgressByStudentIdAsync(userId);

            // Calculate overall statistics
            var totalEvaluations = evaluations.Count;
            var averagePercentage = evaluations.Any()
                ? (double)evaluations.Average(e => e.PercentageObtained ?? 0) // Explicit cast here
                : 0;

            // Group by event type
            var groupedByEvent = evaluations
                .GroupBy(e => e.EventName)
                .Select(g => new EventSummaryDto
                {
                    EventName = g.Key ?? "Unknown",
                    ObtainedMarks = g.Sum(e => e.ObtainedMarks),
                    TotalMarks = g.Sum(e => e.TotalMarks ?? 0),
                    Percentage = g.Any() && g.Sum(e => e.TotalMarks ?? 0) > 0
                        ? (double)((g.Sum(e => e.ObtainedMarks) * 100.0) / g.Sum(e => e.TotalMarks ?? 0)) // Explicit cast here
                        : 0
                })
                .ToList();

            return new StudentPerformanceDashboardDto
            {
                StudentId = userId,
                TotalEvaluations = totalEvaluations,
                AveragePerformance = averagePercentage,
                EventSummaries = groupedByEvent,
                DetailedEvaluations = evaluations
            };
        }

        [HttpGet("final-grade")]
        public async Task<ActionResult<double>> GetFinalGrade()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            return await _evaluationService.CalculateFinalGradeAsync(userId);
        }


        [HttpGet("enhanced-progress")]
        public async Task<ActionResult<List<EnhancedStudentEvaluationDto>>> GetEnhancedStudentProgress()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));

            // Get all student evaluations
            var evaluations = await _unitOfWork.Evaluations.GetStudentEvaluationsByStudentIdAsync(userId);

            // Map to enhanced DTOs with all details
            var result = new List<EnhancedStudentEvaluationDto>();
            foreach (var evaluation in evaluations)
            {
                result.Add(await _evaluationService.MapToEnhancedStudentEvaluationDtoAsync(evaluation));
            }

            return result;
        }

    }


}
