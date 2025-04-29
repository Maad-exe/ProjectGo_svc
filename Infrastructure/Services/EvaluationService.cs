using backend.Core.Entities;
using backend.Core.Entities.PanelManagement;
using backend.DTOs.PanelManagementDTOs;
using backend.UnitOfWork;
using backend.Infrastructure.Services.Contracts;
using backend.UnitOfWork.Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace backend.Infrastructure.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EvaluationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EvaluationEventDto> CreateEventAsync(CreateEventDto eventDto)
        {
            var evaluationEvent = new EvaluationEvent
            {
                Name = eventDto.Name,
                Description = eventDto.Description,
                Date = eventDto.Date,
                TotalMarks = eventDto.TotalMarks,
                Type = eventDto.Type,
                Weight = eventDto.Weight,
                RubricId = eventDto.RubricId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var createdEvent = await _unitOfWork.Evaluations.CreateEventAsync(evaluationEvent);
            await _unitOfWork.SaveChangesAsync();

            return new EvaluationEventDto
            {
                Id = createdEvent.Id,
                Name = createdEvent.Name,
                Description = createdEvent.Description,
                Date = createdEvent.Date,
                TotalMarks = createdEvent.TotalMarks,
                IsActive = createdEvent.IsActive,
                CreatedAt = createdEvent.CreatedAt,
                Type = createdEvent.Type,
                Weight = createdEvent.Weight,
                RubricId = createdEvent.RubricId,
                RubricName = createdEvent.Rubric?.Name
            };
        }

        public async Task<List<StudentDto>> GetStudentsForGroupEvaluationAsync(int groupEvaluationId, int teacherId)
        {
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(groupEvaluationId);
            if (groupEvaluation == null)
                throw new ApplicationException($"Group evaluation with ID {groupEvaluationId} not found");

            var students = new List<StudentDto>();

            foreach (var member in groupEvaluation.Group.Members)
            {
                var student = member.Student;
                var studentEvaluation = groupEvaluation.StudentEvaluations
                    .FirstOrDefault(se => se.StudentId == student.Id);

                bool isEvaluated = false;
                if (studentEvaluation != null)
                {
                    // Check specifically if THIS teacher has evaluated THIS student
                    var hasEvaluated = await _unitOfWork.Evaluations.HasTeacherEvaluatedStudentAsync(teacherId, studentEvaluation.Id);
                    isEvaluated = hasEvaluated;
                }

                students.Add(new StudentDto
                {
                    Id = student.Id,
                    FullName = student.FullName,
                    Email = student.Email,
                    EnrollmentNumber = student.EnrollmentNumber,
                    Department = student.Department,
                    IsEvaluated = isEvaluated
                });
            }

            return students;
        }

        public async Task<EvaluationEventDto?> GetEventByIdAsync(int eventId)
        {
            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(eventId);
            if (evaluationEvent == null)
                return null;

            return new EvaluationEventDto
            {
                Id = evaluationEvent.Id,
                Name = evaluationEvent.Name,
                Description = evaluationEvent.Description,
                Date = evaluationEvent.Date,
                TotalMarks = evaluationEvent.TotalMarks,
                IsActive = evaluationEvent.IsActive,
                CreatedAt = evaluationEvent.CreatedAt,
                Type = evaluationEvent.Type,
                Weight = evaluationEvent.Weight,
                RubricId = evaluationEvent.RubricId,
                RubricName = evaluationEvent.Rubric?.Name
            };
        }

        public async Task<List<EvaluationEventDto>> GetAllEventsAsync()
        {
            var events = await _unitOfWork.Evaluations.GetAllEventsAsync();
            return events.Select(e => new EvaluationEventDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Date = e.Date,
                TotalMarks = e.TotalMarks,
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt,
                Type = e.Type,
                Weight = e.Weight,
                RubricId = e.RubricId,
                RubricName = e.Rubric?.Name
            }).ToList();
        }

        public async Task<EvaluationEventDto> UpdateEventAsync(int eventId, UpdateEventDto eventDto)
        {
            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(eventId);
            if (evaluationEvent == null)
                throw new ApplicationException($"Event with ID {eventId} not found");

            evaluationEvent.Name = eventDto.Name;
            evaluationEvent.Description = eventDto.Description;
            evaluationEvent.Date = eventDto.Date;
            evaluationEvent.TotalMarks = eventDto.TotalMarks;
            evaluationEvent.IsActive = eventDto.IsActive;
            evaluationEvent.Type = eventDto.Type;
            evaluationEvent.Weight = eventDto.Weight;
            evaluationEvent.RubricId = eventDto.RubricId;

            await _unitOfWork.Evaluations.UpdateEventAsync(evaluationEvent);
            await _unitOfWork.SaveChangesAsync();

            return new EvaluationEventDto
            {
                Id = evaluationEvent.Id,
                Name = evaluationEvent.Name,
                Description = evaluationEvent.Description,
                Date = evaluationEvent.Date,
                TotalMarks = evaluationEvent.TotalMarks,
                IsActive = evaluationEvent.IsActive,
                CreatedAt = evaluationEvent.CreatedAt,
                Type = evaluationEvent.Type,
                Weight = evaluationEvent.Weight,
                RubricId = evaluationEvent.RubricId,
                RubricName = evaluationEvent.Rubric?.Name
            };
        }

        public async Task DeleteEventAsync(int eventId)
        {
            await _unitOfWork.Evaluations.DeleteEventAsync(eventId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<GroupEvaluationDto> AssignPanelToGroupAsync(AssignPanelDto assignDto)
        {
            var panel = await _unitOfWork.Panels.GetPanelByIdAsync(assignDto.PanelId);
            if (panel == null)
                throw new ApplicationException($"Panel with ID {assignDto.PanelId} not found");

            var group = await _unitOfWork.Groups.GetGroupByIdAsync(assignDto.GroupId);
            if (group == null)
                throw new ApplicationException($"Group with ID {assignDto.GroupId} not found");

            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(assignDto.EventId);
            if (evaluationEvent == null)
                throw new ApplicationException($"Event with ID {assignDto.EventId} not found");

            if (!group.TeacherId.HasValue)
                throw new ApplicationException("Cannot assign panel to a group without a supervisor");

            var supervisorId = group.TeacherId.Value;
            var supervisorInPanel = await _unitOfWork.Panels.IsTeacherInPanelAsync(assignDto.PanelId, supervisorId);
            if (supervisorInPanel)
                throw new ApplicationException("Cannot assign a panel that includes the group's supervisor");

            var existingEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationsByGroupIdAsync(assignDto.GroupId);
            if (existingEvaluation.Any(ge => ge.EventId == assignDto.EventId))
                throw new ApplicationException($"Group is already assigned for evaluation for this event");

            var groupEvaluation = new GroupEvaluation
            {
                GroupId = assignDto.GroupId,
                PanelId = assignDto.PanelId,
                EventId = assignDto.EventId,
                ScheduledDate = assignDto.ScheduledDate,
                IsCompleted = false,
                Comments = string.Empty
            };

            var createdEvaluation = await _unitOfWork.Evaluations.AssignPanelToGroupAsync(groupEvaluation);
            await _unitOfWork.SaveChangesAsync();

            return await MapGroupEvaluationToDto(createdEvaluation);
        }

        public async Task<GroupEvaluationDto?> GetGroupEvaluationByIdAsync(int groupEvaluationId)
        {
            var evaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(groupEvaluationId);
            if (evaluation == null)
                return null;

            return await MapGroupEvaluationToDto(evaluation);
        }

        public async Task<List<GroupEvaluationDto>> GetGroupEvaluationsByPanelIdAsync(int panelId)
        {
            var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByPanelIdAsync(panelId);
            var result = new List<GroupEvaluationDto>();

            foreach (var evaluation in evaluations)
            {
                result.Add(await MapGroupEvaluationToDto(evaluation));
            }

            return result;
        }

        public async Task<List<GroupEvaluationDto>> GetGroupEvaluationsByEventIdAsync(int eventId)
        {
            var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByEventIdAsync(eventId);
            var result = new List<GroupEvaluationDto>();

            foreach (var evaluation in evaluations)
            {
                result.Add(await MapGroupEvaluationToDto(evaluation));
            }

            return result;
        }

        public async Task<List<GroupEvaluationDto>> GetGroupEvaluationsByTeacherIdAsync(int teacherId)
        {
            var panels = await _unitOfWork.Panels.GetPanelsByTeacherIdAsync(teacherId);

            if (!panels.Any())
                return new List<GroupEvaluationDto>();

            var result = new List<GroupEvaluationDto>();

            foreach (var panel in panels)
            {
                var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByPanelIdAsync(panel.Id);

                foreach (var evaluation in evaluations)
                {
                    result.Add(await MapGroupEvaluationToDto(evaluation));
                }
            }

            return result;
        }

        public async Task<StudentEvaluationDto> EvaluateStudentAsync(EvaluateStudentDto evaluationDto)
        {
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(evaluationDto.GroupEvaluationId);
            if (groupEvaluation == null)
                throw new ApplicationException($"Group evaluation with ID {evaluationDto.GroupEvaluationId} not found");

            // Ensure the evaluation marks are valid
            if (!evaluationDto.ObtainedMarks.HasValue || evaluationDto.ObtainedMarks < 0 || 
                evaluationDto.ObtainedMarks > groupEvaluation.Event.TotalMarks)
                throw new ApplicationException($"Marks must be between 0 and {groupEvaluation.Event.TotalMarks}");

            var existingEvaluation = groupEvaluation.StudentEvaluations
                .FirstOrDefault(se => se.StudentId == evaluationDto.StudentId);

            // Create new evaluation if it doesn't exist
            if (existingEvaluation == null)
            {
                var studentEvaluation = new StudentEvaluation
                {
                    GroupEvaluationId = evaluationDto.GroupEvaluationId,
                    StudentId = evaluationDto.StudentId,
                    ObtainedMarks = evaluationDto.ObtainedMarks ?? 0,
                    Feedback = evaluationDto.Feedback ?? string.Empty,
                    EvaluatedAt = DateTime.Now,
                    RequiredEvaluatorsCount = groupEvaluation.Panel.Members.Count
                };

                var createdEvaluation = await _unitOfWork.Evaluations.EvaluateStudentAsync(studentEvaluation);
                
                // Add the evaluator to the student evaluation
                await _unitOfWork.Evaluations.AddEvaluatorToStudentEvaluationAsync(studentEvaluation.Id, evaluationDto.EvaluatorId);
                
                // Create a simple score entry
                var simpleScore = new StudentCategoryScore
                {
                    StudentEvaluationId = studentEvaluation.Id,
                    CategoryId = null,  // Use null to indicate it's a simple evaluation instead of 0
                    Score = evaluationDto.ObtainedMarks.Value,
                    Feedback = evaluationDto.Feedback ?? string.Empty,
                    EvaluatorId = evaluationDto.EvaluatorId,
                    EvaluatedAt = DateTime.UtcNow
                };
                
                studentEvaluation.CategoryScores.Add(simpleScore);
                await _unitOfWork.SaveChangesAsync();
                
                return await MapToStudentEvaluationDtoAsync(studentEvaluation);
            }
            else
            {
                // Check if this evaluator has already evaluated this student
                var hasEvaluated = await _unitOfWork.Evaluations.HasTeacherEvaluatedStudentAsync(
                    evaluationDto.EvaluatorId, existingEvaluation.Id);
                    
                if (hasEvaluated)
                {
                    // Update existing score
                    var existingScore = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAndEvaluatorIdAsync(
                        existingEvaluation.Id, evaluationDto.EvaluatorId);
                        
                    var simpleScore = existingScore.FirstOrDefault(s => s.CategoryId == null);
                    
                    if (simpleScore != null)
                    {
                        simpleScore.Score = evaluationDto.ObtainedMarks.Value;
                        simpleScore.Feedback = evaluationDto.Feedback ?? string.Empty;
                        simpleScore.EvaluatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Add a new score for this evaluator
                        var newScore = new StudentCategoryScore
                        {
                            StudentEvaluationId = existingEvaluation.Id,
                            CategoryId = null,
                            Score = evaluationDto.ObtainedMarks.Value,
                            Feedback = evaluationDto.Feedback ?? string.Empty,
                            EvaluatorId = evaluationDto.EvaluatorId,
                            EvaluatedAt = DateTime.UtcNow
                        };
                        
                        existingEvaluation.CategoryScores.Add(newScore);
                    }
                    
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    // Add the evaluator to the student evaluation
                    await _unitOfWork.Evaluations.AddEvaluatorToStudentEvaluationAsync(
                        existingEvaluation.Id, evaluationDto.EvaluatorId);
                        
                    // Add a new score for this evaluator
                    var newScore = new StudentCategoryScore
                    {
                        StudentEvaluationId = existingEvaluation.Id,
                        CategoryId = null,
                        Score = evaluationDto.ObtainedMarks.Value,
                        Feedback = evaluationDto.Feedback ?? string.Empty,
                        EvaluatorId = evaluationDto.EvaluatorId,
                        EvaluatedAt = DateTime.UtcNow
                    };
                    
                    existingEvaluation.CategoryScores.Add(newScore);
                    await _unitOfWork.SaveChangesAsync();
                }
                
                // Recalculate the obtained marks based on all evaluators
                var allScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(existingEvaluation.Id);
                existingEvaluation.ObtainedMarks = allScores != null && allScores.Any()
                    ? (int)Math.Round(allScores.Average(s => s.Score))
                    : 0;
                
                // Check if all required evaluators have completed their evaluations
                var uniqueEvaluators = await _unitOfWork.Rubrics.GetUniqueEvaluatorsCountForStudentEvaluationAsync(existingEvaluation.Id);
                if (uniqueEvaluators >= existingEvaluation.RequiredEvaluatorsCount && !existingEvaluation.IsComplete)
                {
                    existingEvaluation.IsComplete = true;
                    await CompileAndUpdateFeedbackAsync(existingEvaluation.Id);
                }
                
                await _unitOfWork.SaveChangesAsync();
                
                return await MapToStudentEvaluationDtoAsync(existingEvaluation);
            }
        }

        public async Task<List<StudentEvaluationDto>> GetStudentEvaluationsByGroupEvaluationIdAsync(int groupEvaluationId)
        {
            var evaluations = await _unitOfWork.Evaluations.GetStudentEvaluationsByGroupEvaluationIdAsync(groupEvaluationId);

            return evaluations.Select(e => new StudentEvaluationDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                StudentName = e.Student.FullName,
                ObtainedMarks = e.ObtainedMarks,
                Feedback = e.Feedback,
                EvaluatedAt = e.EvaluatedAt
            }).ToList();
        }

        public async Task<List<StudentEvaluationDto>> GetStudentProgressByStudentIdAsync(int studentId)
        {
            var evaluations = await _unitOfWork.Evaluations.GetStudentEvaluationsByStudentIdAsync(studentId);

            return evaluations.Select(e => new StudentEvaluationDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                StudentName = string.Empty,
                ObtainedMarks = e.ObtainedMarks,
                Feedback = e.Feedback,
                EvaluatedAt = e.EvaluatedAt,
                EventName = e.GroupEvaluation.Event.Name,
                EventDate = e.GroupEvaluation.Event.Date,
                TotalMarks = e.GroupEvaluation.Event.TotalMarks,
                PercentageObtained = (e.ObtainedMarks * 100) / e.GroupEvaluation.Event.TotalMarks
            }).ToList();
        }

        private async Task<GroupEvaluationDto> MapGroupEvaluationToDto(GroupEvaluation evaluation)
        {
            var group = await _unitOfWork.Groups.GetGroupByIdAsync(evaluation.GroupId);
            var panel = await _unitOfWork.Panels.GetPanelByIdAsync(evaluation.PanelId);
            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(evaluation.EventId);

            var studentEvaluations = await _unitOfWork.Evaluations.GetStudentEvaluationsByGroupEvaluationIdAsync(evaluation.Id);

            return new GroupEvaluationDto
            {
                Id = evaluation.Id,
                GroupId = evaluation.GroupId,
                GroupName = group?.Name ?? "Unknown Group",
                PanelId = evaluation.PanelId,
                PanelName = panel?.Name ?? "Unknown Panel",
                EventId = evaluation.EventId,
                EventName = evaluationEvent?.Name ?? "Unknown Event",
                ScheduledDate = evaluation.ScheduledDate,
                IsCompleted = evaluation.IsCompleted,
                Comments = evaluation.Comments,
                StudentEvaluations = studentEvaluations.Select(se => new StudentEvaluationDto
                {
                    Id = se.Id,
                    StudentId = se.StudentId,
                    StudentName = se.Student.FullName,
                    ObtainedMarks = se.ObtainedMarks,
                    Feedback = se.Feedback,
                    EvaluatedAt = se.EvaluatedAt
                }).ToList()
            };
        }

        public async Task<List<GroupPerformanceDto>> GetSupervisedGroupsPerformanceAsync(int teacherId)
        {
            var supervisedGroups = await _unitOfWork.Groups.GetGroupsByTeacherIdAsync(teacherId);

            if (!supervisedGroups.Any())
                return new List<GroupPerformanceDto>();

            var result = new List<GroupPerformanceDto>();

            foreach (var group in supervisedGroups)
            {
                var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByGroupIdAsync(group.Id);

                var groupPerformance = new GroupPerformanceDto
                {
                    GroupId = group.Id,
                    GroupName = group.Name,
                    Events = new List<EventPerformanceDto>(),
                    CompletedEvents = evaluations.Count(e => e.IsCompleted),
                    TotalEvents = evaluations.Count
                };

                double totalPercentage = 0;
                int evaluationCount = 0;

                foreach (var evaluation in evaluations)
                {
                    var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(evaluation.EventId);
                    if (evaluationEvent == null) continue;

                    var studentEvaluations = await _unitOfWork.Evaluations.GetStudentEvaluationsByGroupEvaluationIdAsync(evaluation.Id);

                    if (!studentEvaluations.Any())
                        continue;

                    var totalMarks = studentEvaluations.Sum(se => se.ObtainedMarks);
                    var averageMarks = totalMarks / (double)studentEvaluations.Count;
                    var percentage = (averageMarks * 100) / evaluationEvent.TotalMarks;

                    totalPercentage += percentage;
                    evaluationCount++;

                    var eventPerformance = new EventPerformanceDto
                    {
                        EventId = evaluationEvent.Id,
                        EventName = evaluationEvent.Name,
                        AverageMarks = averageMarks,
                        TotalMarks = evaluationEvent.TotalMarks,
                        Percentage = percentage,
                        EvaluatedOn = evaluation.ScheduledDate,
                        IsCompleted = evaluation.IsCompleted,
                        StudentPerformances = new List<StudentPerformanceDto>()
                    };

                    foreach (var se in studentEvaluations)
                    {
                        var student = await _unitOfWork.Students.GetStudentByIdAsync(se.StudentId);
                        if (student == null) continue;

                        eventPerformance.StudentPerformances.Add(new StudentPerformanceDto
                        {
                            StudentId = se.StudentId,
                            StudentName = student.FullName,
                            ObtainedMarks = se.ObtainedMarks,
                            Percentage = (se.ObtainedMarks * 100.0) / evaluationEvent.TotalMarks,
                            Feedback = se.Feedback
                        });
                    }
                    groupPerformance.Events.Add(eventPerformance);
                }

                groupPerformance.AveragePerformance = evaluationCount > 0 ? totalPercentage / evaluationCount : 0;

                result.Add(groupPerformance);
            }

            return result;
        }

        public async Task<TeacherDashboardDto> GetTeacherDashboardAsync(int teacherId)
        {
            var supervisedGroups = await _unitOfWork.Groups.GetGroupsByTeacherIdAsync(teacherId);

            var panels = await _unitOfWork.Panels.GetPanelsByTeacherIdAsync(teacherId);

            int totalEvaluationsCount = 0;
            foreach (var panel in panels)
            {
                var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByPanelIdAsync(panel.Id);
                totalEvaluationsCount += evaluations.Count;
            }

            var supervisedGroupsPerformance = await GetSupervisedGroupsPerformanceAsync(teacherId);

            return new TeacherDashboardDto
            {
                SupervisedGroupCount = supervisedGroups.Count(),
                PanelMembershipCount = panels.Count,
                TotalEvaluationsCount = totalEvaluationsCount,
                SupervisedGroups = supervisedGroupsPerformance
            };
        }

        public async Task<GroupEvaluationDto> UpdateGroupEvaluationCommentsAsync(int groupEvaluationId, string comments)
        {
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(groupEvaluationId);
            if (groupEvaluation == null)
                throw new ApplicationException($"Group evaluation with ID {groupEvaluationId} not found");

            groupEvaluation.Comments = comments;
            await _unitOfWork.SaveChangesAsync();

            return await MapGroupEvaluationToDto(groupEvaluation);
        }

        public async Task<GroupEvaluationDto> UpdateGroupEvaluationAsync(int groupEvaluationId, GroupEvaluation updatedEvaluation)
        {
            var existingEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(groupEvaluationId);
            if (existingEvaluation == null)
                throw new ApplicationException($"Group evaluation with ID {groupEvaluationId} not found");

            existingEvaluation.ScheduledDate = updatedEvaluation.ScheduledDate;
            existingEvaluation.IsCompleted = updatedEvaluation.IsCompleted;
            existingEvaluation.Comments = updatedEvaluation.Comments;

            await _unitOfWork.SaveChangesAsync();

            return await MapGroupEvaluationToDto(existingEvaluation);
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var panels = await _unitOfWork.Panels.GetAllPanelsAsync();
            var events = await _unitOfWork.Evaluations.GetAllEventsAsync();
            var allGroups = await _unitOfWork.Groups.GetAllGroupsAsync();
            var supervisedGroups = allGroups.Where(g => g.TeacherId.HasValue).ToList();

            var allEvaluations = new List<GroupEvaluation>();
            foreach (var e in events)
            {
                var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByEventIdAsync(e.Id);
                allEvaluations.AddRange(evaluations);
            }

            var eventStats = new List<EventStatisticsDto>();
            foreach (var evt in events)
            {
                var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByEventIdAsync(evt.Id);

                double averagePerformance = 0;
                int evaluatedGroupsCount = 0;

                foreach (var eval in evaluations)
                {
                    var studentEvaluations = await _unitOfWork.Evaluations.GetStudentEvaluationsByGroupEvaluationIdAsync(eval.Id);
                    if (studentEvaluations == null || !studentEvaluations.Any())
                        continue;

                    double groupAvg = studentEvaluations.Average(se =>
                        (se.ObtainedMarks * 100.0) / evt.TotalMarks);
                    averagePerformance += groupAvg;
                    evaluatedGroupsCount++;
                }

                if (evaluatedGroupsCount > 0)
                {
                    averagePerformance /= evaluatedGroupsCount;
                }

                eventStats.Add(new EventStatisticsDto
                {
                    EventId = evt.Id,
                    EventName = evt.Name,
                    TotalGroups = evaluations.Count,
                    EvaluatedGroups = evaluatedGroupsCount,
                    AveragePerformance = averagePerformance,
                    Date = evt.Date
                });
            }

            return new AdminDashboardDto
            {
                TotalPanels = panels.Count,
                TotalEvents = events.Count,
                TotalGroups = allGroups.Count,
                SupervisedGroups = supervisedGroups.Count,
                CompletedEvaluations = allEvaluations.Count(e => e.IsCompleted),
                PendingEvaluations = allEvaluations.Count(e => !e.IsCompleted),
                EventStatistics = eventStats
            };
        }

        public async Task<EvaluationRubricDto> CreateRubricAsync(CreateRubricDto rubricDto)
        {
            double totalWeight = rubricDto.Categories.Sum(c => c.Weight);
            if (Math.Abs(totalWeight - 1.0) > 0.01)
            {
                throw new ApplicationException("Category weights must sum to 1.0 (100%)");
            }

            var rubric = new EvaluationRubric
            {
                Name = rubricDto.Name,
                Description = rubricDto.Description,
                IsActive = true,
                CreatedAt = DateTime.Now,
                Categories = rubricDto.Categories.Select(c => new RubricCategory
                {
                    Name = c.Name,
                    Description = c.Description,
                    Weight = c.Weight,
                    MaxScore = c.MaxScore
                }).ToList()
            };

            var createdRubric = await _unitOfWork.Rubrics.CreateRubricAsync(rubric);
            await _unitOfWork.SaveChangesAsync();

            return await MapRubricToDto(createdRubric);
        }

        public async Task<List<EvaluationRubricDto>> GetAllRubricsAsync()
        {
            var rubrics = await _unitOfWork.Rubrics.GetAllRubricsAsync();
            var result = new List<EvaluationRubricDto>();

            foreach (var rubric in rubrics)
            {
                result.Add(await MapRubricToDto(rubric));
            }

            return result;
        }

        public async Task<EvaluationRubricDto?> GetRubricByIdAsync(int rubricId)
        {
            var rubric = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(rubricId);
            if (rubric == null)
                return null;

            return await MapRubricToDto(rubric);
        }

        public async Task<EvaluationRubricDto> UpdateRubricAsync(int rubricId, UpdateRubricDto rubricDto)
        {
            var rubric = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(rubricId);
            if (rubric == null)
                throw new ApplicationException($"Rubric with ID {rubricId} not found");

            double totalWeight = rubricDto.Categories.Sum(c => c.Weight);
            if (Math.Abs(totalWeight - 1.0) > 0.01)
            {
                throw new ApplicationException("Category weights must sum to 1.0 (100%)");
            }

            rubric.Name = rubricDto.Name;
            rubric.Description = rubricDto.Description;
            rubric.IsActive = rubricDto.IsActive;

            foreach (var categoryDto in rubricDto.Categories)
            {
                var category = rubric.Categories.FirstOrDefault(c => c.Id == categoryDto.Id);

                if (category != null)
                {
                    category.Name = categoryDto.Name;
                    category.Description = categoryDto.Description;
                    category.Weight = categoryDto.Weight;
                    category.MaxScore = categoryDto.MaxScore;
                }
                else
                {
                    rubric.Categories.Add(new RubricCategory
                    {
                        RubricId = rubric.Id,
                        Name = categoryDto.Name,
                        Description = categoryDto.Description,
                        Weight = categoryDto.Weight,
                        MaxScore = categoryDto.MaxScore
                    });
                }
            }

            var categoryIdsToKeep = rubricDto.Categories.Select(c => c.Id).ToList();
            var categoriesToRemove = rubric.Categories
                .Where(c => c.Id > 0 && !categoryIdsToKeep.Contains(c.Id))
                .ToList();

            foreach (var categoryToRemove in categoriesToRemove)
            {
                rubric.Categories.Remove(categoryToRemove);
            }

            await _unitOfWork.Rubrics.UpdateRubricAsync(rubric);
            await _unitOfWork.SaveChangesAsync();

            return await MapRubricToDto(rubric);
        }

        public async Task DeleteRubricAsync(int rubricId)
        {
            var events = await _unitOfWork.Evaluations.GetAllEventsAsync();
            var isRubricInUse = events.Any(e => e.RubricId == rubricId);

            if (isRubricInUse)
            {
                throw new ApplicationException("Cannot delete rubric that is in use by evaluation events");
            }

            await _unitOfWork.Rubrics.DeleteRubricAsync(rubricId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<EnhancedStudentEvaluationDto> EvaluateStudentWithRubricAsync(int teacherId, EvaluateStudentDto evaluationDto)
        {
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(evaluationDto.GroupEvaluationId);
            if (groupEvaluation == null)
                throw new ApplicationException($"Group evaluation with ID {evaluationDto.GroupEvaluationId} not found");

            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(groupEvaluation.EventId);
            if (evaluationEvent == null)
                throw new ApplicationException("Evaluation event not found");

            var student = await _unitOfWork.Students.GetStudentByIdAsync(evaluationDto.StudentId);
            if (student == null)
                throw new ApplicationException($"Student with ID {evaluationDto.StudentId} not found");

            var studentInGroup = groupEvaluation.Group.Members.Any(m => m.StudentId == evaluationDto.StudentId);
            if (!studentInGroup)
                throw new ApplicationException("Student is not a member of the group being evaluated");

            var panel = await _unitOfWork.Panels.GetPanelByIdAsync(groupEvaluation.PanelId);
            if (panel == null)
                throw new ApplicationException("Panel not found for this group evaluation");

            var studentEvaluation = groupEvaluation.StudentEvaluations
                .FirstOrDefault(se => se.StudentId == evaluationDto.StudentId);

            if (studentEvaluation == null)
            {
                studentEvaluation = new StudentEvaluation
                {
                    GroupEvaluationId = evaluationDto.GroupEvaluationId,
                    StudentId = evaluationDto.StudentId,
                    RubricId = evaluationEvent.RubricId,
                    ObtainedMarks = 0,
                    Feedback = string.Empty,
                    EvaluatedAt = DateTime.UtcNow,
                    IsComplete = false,
                    RequiredEvaluatorsCount = panel.Members.Count
                };

                groupEvaluation.StudentEvaluations.Add(studentEvaluation);
                await _unitOfWork.SaveChangesAsync();
            }

            var existingScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAndEvaluatorIdAsync(
                studentEvaluation.Id, teacherId);

            bool hasEvaluatedAllCategories = false;

            if (evaluationEvent.RubricId.HasValue)
            {
                var rubric = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(evaluationEvent.RubricId.Value);
                if (rubric == null)
                    throw new ApplicationException("Rubric not found for this evaluation event");

                var teacherScores = existingScores.ToList();
                var allCategories = rubric.Categories.Select(c => c.Id).ToList();

                hasEvaluatedAllCategories = allCategories.All(categoryId =>
                    teacherScores.Any(ts => ts.CategoryId == categoryId));
            }

            if (hasEvaluatedAllCategories)
            {
                Console.WriteLine($"Teacher {teacherId} is updating their previous evaluation for student {evaluationDto.StudentId}");
            }

            if (evaluationDto.CategoryScores != null)
            {
                if (!evaluationEvent.RubricId.HasValue)
                    throw new ApplicationException("Cannot submit category scores for a non-rubric evaluation");

                var rubric = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(evaluationEvent.RubricId.Value);
                if (rubric == null)
                    throw new ApplicationException("Rubric not found for this evaluation event");

                foreach (var scoreDto in evaluationDto.CategoryScores)
                {
                    var category = rubric.Categories.FirstOrDefault(c => c.Id == scoreDto.CategoryId);
                    if (category == null) continue;

                    if (scoreDto.Score < 0 || scoreDto.Score > category.MaxScore)
                        throw new ApplicationException($"Score for category {category.Name} must be between 0 and {category.MaxScore}");

                    var existingScore = existingScores.FirstOrDefault(s => s.CategoryId == scoreDto.CategoryId);

                    if (existingScore != null)
                    {
                        existingScore.Score = scoreDto.Score;
                        existingScore.Feedback = scoreDto.Feedback;
                        existingScore.EvaluatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var score = new StudentCategoryScore
                        {
                            StudentEvaluationId = studentEvaluation.Id,
                            CategoryId = scoreDto.CategoryId,
                            Score = scoreDto.Score,
                            Feedback = scoreDto.Feedback,
                            EvaluatorId = teacherId,
                            EvaluatedAt = DateTime.UtcNow
                        };

                        studentEvaluation.CategoryScores.Add(score);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                var uniqueEvaluators = await _unitOfWork.Rubrics.GetUniqueEvaluatorsCountForStudentEvaluationAsync(studentEvaluation.Id);

                bool allRequiredEvaluatorsCompleted = uniqueEvaluators >= studentEvaluation.RequiredEvaluatorsCount;

                if (allRequiredEvaluatorsCompleted && !studentEvaluation.IsComplete)
                {
                    studentEvaluation.IsComplete = true;

                    await CalculateObtainedMarksFromCategoryScores(studentEvaluation.Id, evaluationEvent);

                    await _unitOfWork.SaveChangesAsync();

                    await CompileAndUpdateFeedbackAsync(studentEvaluation.Id);
                }
            }
            else if (evaluationDto.ObtainedMarks.HasValue)
            {
                if (evaluationDto.ObtainedMarks < 0 || evaluationDto.ObtainedMarks > evaluationEvent.TotalMarks)
                    throw new ApplicationException($"Marks must be between 0 and {evaluationEvent.TotalMarks}");

                await _unitOfWork.Evaluations.AddEvaluatorToStudentEvaluationAsync(studentEvaluation.Id, teacherId);

                var simpleScore = new StudentCategoryScore
                {
                    StudentEvaluationId = studentEvaluation.Id,
                    CategoryId = null,
                    Score = evaluationDto.ObtainedMarks.Value,
                    Feedback = evaluationDto.Feedback ?? string.Empty,
                    EvaluatorId = teacherId,
                    EvaluatedAt = DateTime.UtcNow
                };

                var existingSimpleScore = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAndEvaluatorIdAsync(
                    studentEvaluation.Id, teacherId);

                var existingScore = existingSimpleScore.FirstOrDefault(s => s.CategoryId == null);

                if (existingScore != null)
                {
                    existingScore.Score = evaluationDto.ObtainedMarks.Value;
                    existingScore.Feedback = evaluationDto.Feedback ?? string.Empty;
                    existingScore.EvaluatedAt = DateTime.UtcNow;
                }
                else
                {
                    studentEvaluation.CategoryScores.Add(simpleScore);
                }

                await _unitOfWork.SaveChangesAsync();

                var uniqueEvaluators = await _unitOfWork.Rubrics.GetUniqueEvaluatorsCountForStudentEvaluationAsync(studentEvaluation.Id);
                bool allRequiredEvaluatorsCompleted = uniqueEvaluators >= studentEvaluation.RequiredEvaluatorsCount;

                if (allRequiredEvaluatorsCompleted && !studentEvaluation.IsComplete)
                {
                    studentEvaluation.IsComplete = true;

                    var allScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(studentEvaluation.Id);
                    studentEvaluation.ObtainedMarks = allScores != null && allScores.Any()
                        ? (int)Math.Round(allScores.Average(s => s.Score))
                        : 0;

                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return await MapToEnhancedStudentEvaluationDtoAsync(studentEvaluation);
        }

        private async Task CompileAndUpdateFeedbackAsync(int studentEvaluationId)
        {
            var studentEvaluation = await _unitOfWork.Evaluations.GetStudentEvaluationByIdAsync(studentEvaluationId);
            if (studentEvaluation == null) return;

            var categoryScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(studentEvaluationId);
            if (categoryScores == null || !categoryScores.Any()) return;

            var evaluators = await _unitOfWork.Evaluations.GetEvaluatorsByStudentEvaluationIdAsync(studentEvaluationId);
            if (evaluators == null || !evaluators.Any()) return;

            var feedbackByEvaluator = new Dictionary<int, List<string>>();
            var evaluatorMap = new Dictionary<int, string>();
            
            // Create evaluator name map
            foreach (var evaluator in evaluators)
            {
                // Ensure we have a full name for every evaluator
                var teacherInfo = await _unitOfWork.Teachers.GetTeacherByIdAsync(evaluator.Id);
                evaluatorMap[evaluator.Id] = teacherInfo?.FullName ?? $"Evaluator {evaluator.Id}";
                feedbackByEvaluator[evaluator.Id] = new List<string>();
            }

            foreach (var score in categoryScores.Where(s => s.EvaluatorId > 0))
            {
                if (!feedbackByEvaluator.ContainsKey(score.EvaluatorId))
                {
                    feedbackByEvaluator[score.EvaluatorId] = new List<string>();
                    
                    // If this evaluator wasn't in our original list, fetch their name
                    if (!evaluatorMap.ContainsKey(score.EvaluatorId))
                    {
                        var teacherInfo = await _unitOfWork.Teachers.GetTeacherByIdAsync(score.EvaluatorId);
                        evaluatorMap[score.EvaluatorId] = teacherInfo?.FullName ?? $"Evaluator {score.EvaluatorId}";
                    }
                }

                var category = score.Category;
                var feedbackText = score.Feedback ?? string.Empty;

                // Ensure feedback is meaningful - we'll include the score
                if (!string.IsNullOrWhiteSpace(feedbackText))
                {
                    if (category != null)
                    {
                        feedbackByEvaluator[score.EvaluatorId].Add($"{category.Name}: {feedbackText}");
                    }
                    else
                    {
                        feedbackByEvaluator[score.EvaluatorId].Add($"{score.Score} - {feedbackText}");
                    }
                }
                else
                {
                    // If no feedback provided, at least include the score
                    feedbackByEvaluator[score.EvaluatorId].Add($"Score: {score.Score}");
                }
            }

            // Compile feedback with evaluator names
            var compiledFeedback = new StringBuilder();
            foreach (var kvp in feedbackByEvaluator)
            {
                if (!kvp.Value.Any()) continue;

                var evaluatorName = evaluatorMap.ContainsKey(kvp.Key) ? evaluatorMap[kvp.Key] : $"Evaluator {kvp.Key}";
                compiledFeedback.AppendLine($"Feedback from {evaluatorName}:");
                
                foreach (var feedback in kvp.Value)
                {
                    compiledFeedback.AppendLine($"- {feedback}");
                }
                compiledFeedback.AppendLine();
            }

            studentEvaluation.Feedback = compiledFeedback.ToString().Trim();
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task CalculateObtainedMarksFromCategoryScores(int studentEvaluationId, EvaluationEvent evaluationEvent)
        {
            var studentEvaluation = await _unitOfWork.Evaluations.GetStudentEvaluationByIdAsync(studentEvaluationId);
            if (studentEvaluation == null) return;

            if (!evaluationEvent.RubricId.HasValue)
            {
                return;
            }

            var rubric = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(evaluationEvent.RubricId.Value);
            if (rubric == null) return;

            var allScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(studentEvaluationId);
            if (allScores == null)
            {
                allScores = new List<StudentCategoryScore>();
            }

            var evaluatorIds = allScores.Select(s => s.EvaluatorId).Distinct().ToList();
            if (evaluatorIds.Count == 0) return;

            double totalScore = 0;
            foreach (var evaluatorId in evaluatorIds)
            {
                var evaluatorScores = allScores.Where(s => s.EvaluatorId == evaluatorId).ToList();

                double evaluatorWeightedTotal = 0;
                double totalWeight = 0;

                foreach (var category in rubric.Categories)
                {
                    if (category == null) continue;

                    var score = evaluatorScores.FirstOrDefault(s => s.CategoryId == category.Id);
                    if (score != null)
                    {
                        double scorePercentage = (double)score.Score / category.MaxScore;
                        evaluatorWeightedTotal += scorePercentage * category.Weight;
                        totalWeight += category.Weight;
                    }
                }

                double evaluatorScore = 0;
                if (totalWeight > 0)
                {
                    double weightedPercentage = evaluatorWeightedTotal / totalWeight;
                    evaluatorScore = weightedPercentage * 100;
                }

                totalScore += evaluatorScore;
            }

            double averageScore = evaluatorIds.Count > 0 ? totalScore / evaluatorIds.Count : 0;

            int calculatedFinalScore = (int)Math.Round((averageScore / 100) * evaluationEvent.TotalMarks);

            studentEvaluation.ObtainedMarks = calculatedFinalScore;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<EnhancedStudentEvaluationDto> MapToEnhancedStudentEvaluationDtoAsync(StudentEvaluation evaluation)
        {
            var student = await _unitOfWork.Students.GetStudentByIdAsync(evaluation.StudentId);
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(evaluation.GroupEvaluationId);
            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(groupEvaluation?.EventId ?? 0);
            var panel = groupEvaluation != null ? await _unitOfWork.Panels.GetPanelByIdAsync(groupEvaluation.PanelId) : null;

            int totalMarks = evaluationEvent?.TotalMarks ?? 0;
            double eventWeight = evaluationEvent?.Weight ?? 1.0;

            decimal? percentageObtained = totalMarks > 0
                ? (decimal?)((evaluation.ObtainedMarks * 100.0) / totalMarks)
                : 0;

            double weightedScore = totalMarks > 0
                ? (evaluation.ObtainedMarks * 100.0 * eventWeight) / totalMarks
                : 0;

            // Get all evaluators first from both categories scores and direct evaluators
            var allCategoryScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(evaluation.Id);
            
            // Get the formal list of evaluators that have been attached to this evaluation
            var evaluators = await _unitOfWork.Evaluations.GetEvaluatorsByStudentEvaluationIdAsync(evaluation.Id);
            
            // Create a combined list of all possible evaluator IDs
            var allEvaluatorIds = new HashSet<int>();
            
            // Add all evaluators from category scores
            if (allCategoryScores != null)
            {
                foreach (var score in allCategoryScores)
                {
                    allEvaluatorIds.Add(score.EvaluatorId);
                }
            }
            
            // Add all formal evaluators
            foreach (var evaluator in evaluators)
            {
                allEvaluatorIds.Add(evaluator.Id);
            }
            
            // Create a mapping of evaluator IDs to their full names
            var evaluatorMap = new Dictionary<int, string>();
            foreach (var evaluatorId in allEvaluatorIds)
            {
                var teacher = await _unitOfWork.Teachers.GetTeacherByIdAsync(evaluatorId);
                if (teacher != null)
                {
                    evaluatorMap[evaluatorId] = teacher.FullName;
                }
                else
                {
                    evaluatorMap[evaluatorId] = $"Evaluator {evaluatorId}"; // Default name if not found
                }
            }

            // Create the DTO with empty collections initially
            var dto = new EnhancedStudentEvaluationDto
            {
                Id = evaluation.Id,
                StudentId = evaluation.StudentId,
                StudentName = student?.FullName ?? "Unknown",
                ObtainedMarks = evaluation.ObtainedMarks,
                Feedback = evaluation.Feedback,
                EvaluatedAt = evaluation.EvaluatedAt,
                EventName = evaluationEvent?.Name,
                EventDate = evaluationEvent?.Date,
                TotalMarks = totalMarks,
                PercentageObtained = percentageObtained,
                WeightedScore = weightedScore,
                CategoryScores = new List<CategoryScoreDetailDto>(),
                Evaluators = new List<EvaluatorDto>(),
                RequiredEvaluatorsCount = panel?.Members?.Count ?? 3,
                EventWeight = eventWeight,
            };

            // Add all evaluators to the result, with correct status based on scores
            foreach (var evaluatorId in allEvaluatorIds)
            {
                var hasEvaluated = allCategoryScores != null && 
                    allCategoryScores.Any(s => s.EvaluatorId == evaluatorId);
                    
                var evaluatorScores = allCategoryScores?
                    .Where(s => s.EvaluatorId == evaluatorId)
                    .ToList();
                    
                var averageScore = evaluatorScores != null && evaluatorScores.Any()
                    ? (int)Math.Round(evaluatorScores.Average(s => s.Score))
                    : 0;
                    
                dto.Evaluators.Add(new EvaluatorDto
                {
                    Id = evaluatorId,
                    Name = evaluatorMap.ContainsKey(evaluatorId) ? evaluatorMap[evaluatorId] : $"Evaluator {evaluatorId}",
                    HasEvaluated = hasEvaluated,
                    Score = averageScore
                });
            }

            // Calculate whether evaluation is complete based on unique evaluator count
            // compared to required evaluator count
            int requiredEvaluators = evaluation.RequiredEvaluatorsCount > 0 ? evaluation.RequiredEvaluatorsCount : (panel?.Members?.Count ?? 3);
            bool isComplete = allEvaluatorIds.Count >= requiredEvaluators || evaluation.IsComplete;
            dto.IsComplete = isComplete;

            if (evaluation.RubricId.HasValue)
            {
                var categoryScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(evaluation.Id);

                if (categoryScores != null)
                {
                    var scoresByCategory = categoryScores.GroupBy(s => s.CategoryId);

                    foreach (var categoryGroup in scoresByCategory)
                    {
                        var firstScore = categoryGroup.First();
                        var category = firstScore.Category;

                        double averageScore = categoryGroup.Any() 
                            ? categoryGroup.Average(s => s.Score)
                            : 0;
                        var categoryWeight = category?.Weight ?? 0;
                        var categoryMaxScore = category?.MaxScore ?? 1;

                        var feedbackItems = new List<string>();
                        foreach (var score in categoryGroup.Where(s => !string.IsNullOrEmpty(s.Feedback)))
                        {
                            var evaluatorName = evaluatorMap[score.EvaluatorId];
                            feedbackItems.Add($"{score.Feedback} (by {evaluatorName})");
                        }
                        
                        var categoryDto = new CategoryScoreDetailDto
                        {
                            CategoryId = category?.Id ?? 0,
                            CategoryName = category?.Name ?? "Unknown",
                            CategoryWeight = categoryWeight,
                            Score = (int)Math.Round(averageScore),
                            MaxScore = categoryMaxScore,
                            WeightedScore = category != null && category.MaxScore > 0
                                ? (averageScore * category.Weight) / category.MaxScore * 100
                                : 0,
                            Feedback = string.Join("\n", feedbackItems),
                            EvaluatorDetails = new List<CategoryEvaluatorDetailDto>()
                        };

                        foreach (var score in categoryGroup)
                        {
                            // Use our evaluator map to get the correct name
                            var evaluatorName = evaluatorMap[score.EvaluatorId];

                            categoryDto.EvaluatorDetails.Add(new CategoryEvaluatorDetailDto
                            {
                                EvaluatorId = score.EvaluatorId,
                                EvaluatorName = evaluatorName,
                                Score = score.Score,
                                Feedback = score.Feedback ?? "",
                                EvaluatedAt = score.EvaluatedAt
                            });
                        }

                        dto.CategoryScores.Add(categoryDto);
                    }
                }
            }

            var evaluatorIds = await _unitOfWork.Evaluations.GetEvaluatorsByStudentEvaluationIdAsync(evaluation.Id);
            var allScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(evaluation.Id);
            
            // Add evaluators with correct status to the result
            foreach (var evaluator in evaluatorIds)
            {
                var hasEvaluated = allScores != null && 
                    allScores.Any(s => s.EvaluatorId == evaluator.Id);
                    
                var evaluatorScore = allScores
                    ?.Where(s => s.EvaluatorId == evaluator.Id)
                    ?.Average(s => s.Score) ?? 0;
                    
                dto.Evaluators.Add(new EvaluatorDto
                {
                    Id = evaluator.Id,
                    Name = evaluator.FullName,
                    HasEvaluated = hasEvaluated, // Properly mark as evaluated
                    Score = (int)evaluatorScore
                });
            }

            return dto;
        }

        private async Task<EvaluationRubricDto> MapRubricToDto(EvaluationRubric rubric)
        {
            var rubricWithCategories = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(rubric.Id);
            if (rubricWithCategories != null)
            {
                rubric = rubricWithCategories;
            }

            return new EvaluationRubricDto
            {
                Id = rubric.Id,
                Name = rubric.Name,
                Description = rubric.Description,
                IsActive = rubric.IsActive,
                CreatedAt = rubric.CreatedAt,
                Categories = rubric.Categories.Select(c => new RubricCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Weight = c.Weight,
                    MaxScore = c.MaxScore
                }).ToList()
            };
        }

        public async Task<double> CalculateFinalGradeAsync(int studentId)
        {
            return await _unitOfWork.Evaluations.CalculateFinalGradeAsync(studentId);
        }

        public async Task<List<NormalizedGradeDto>> GetNormalizedGradesAsync()
        {
            var allEvaluations = await _unitOfWork.Evaluations.GetAllStudentEvaluationsForNormalizationAsync();

            var studentGroups = allEvaluations
                .GroupBy(se => se.StudentId)
                .ToList();

            if (!studentGroups.Any())
                return new List<NormalizedGradeDto>();

            var rawScores = new List<(int StudentId, string StudentName, double Score)>();

            foreach (var group in studentGroups)
            {
                var studentId = group.Key;
                var student = await _unitOfWork.Students.GetStudentByIdAsync(studentId);
                if (student == null) continue;

                double finalGrade = 0;
                try
                {
                    finalGrade = await CalculateFinalGradeAsync(studentId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calculating final grade: {ex.Message}");
                }
                rawScores.Add((studentId, student?.FullName ?? "Unknown", finalGrade));
            }

            double mean = rawScores.Average(s => s.Score);
            double stdDev = Math.Sqrt(rawScores.Average(s => Math.Pow(s.Score - mean, 2)));

            var normalizedScores = rawScores
                .Select(s => new NormalizedGradeDto
                {
                    StudentId = s.StudentId,
                    StudentName = s.StudentName,
                    RawGrade = s.Score,
                    NormalizedGrade = stdDev > 0
                        ? Math.Min(100, Math.Max(0, ((s.Score - mean) / stdDev) * 10 + 70))
                        : s.Score
                })
                .OrderByDescending(s => s.NormalizedGrade)
                .ToList();

            return normalizedScores;
        }

        public async Task<StudentEvaluationDto> GetEvaluationByIdAsync(int evaluationId)
        {
            var evaluation = await _unitOfWork.Evaluations.GetStudentEvaluationByIdAsync(evaluationId);
            if (evaluation == null)
                throw new ApplicationException($"Evaluation with ID {evaluationId} not found");

            var student = await _unitOfWork.Students.GetStudentByIdAsync(evaluation.StudentId);

            return new StudentEvaluationDto
            {
                Id = evaluation.Id,
                StudentId = evaluation.StudentId,
                StudentName = student?.FullName ?? "Unknown",
                ObtainedMarks = evaluation.ObtainedMarks,
                Feedback = evaluation.Feedback,
                EvaluatedAt = evaluation.EvaluatedAt,
                IsComplete = evaluation.IsComplete,
                EventName = evaluation.GroupEvaluation?.Event?.Name,
                EventDate = evaluation.GroupEvaluation?.Event?.Date,
                TotalMarks = evaluation.GroupEvaluation?.Event?.TotalMarks
            };
        }

        public async Task<bool> MarkEvaluationAsCompleteAsync(int evaluationId)
        {
            var evaluation = await _unitOfWork.Evaluations.GetStudentEvaluationByIdAsync(evaluationId);
            if (evaluation == null)
                throw new ApplicationException($"Evaluation with ID {evaluationId} not found");

            await _unitOfWork.Evaluations.MarkEvaluationAsCompleteAsync(evaluationId);

            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(evaluation.GroupEvaluationId);
            if (groupEvaluation?.IsCompleted != true)
            {
                var groupMembers = groupEvaluation.Group.Members.Count();
                var completedEvaluations = groupEvaluation.StudentEvaluations.Count(se => se.IsComplete);

                if (groupMembers == completedEvaluations)
                {
                    groupEvaluation.IsCompleted = true;
                    await _unitOfWork.Evaluations.UpdateGroupEvaluationAsync(groupEvaluation);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task UpdateFinalMarks(StudentEvaluation evaluation)
        {
            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(evaluation.GroupEvaluation.EventId);
            if (evaluationEvent == null) return;

            var allScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(evaluation.Id);
            if (allScores == null || !allScores.Any()) return;

            if (evaluationEvent.RubricId.HasValue)
            {
                await CalculateObtainedMarksFromCategoryScores(evaluation.Id, evaluationEvent);
            }
            else
            {
                evaluation.ObtainedMarks = allScores != null && allScores.Any()
                    ? (int)Math.Round(allScores.Average(s => s.Score))
                    : 0;
            }
        }

        public async Task<EvaluationStatisticsDto> GetEvaluationStatisticsAsync(int groupEvaluationId, int studentId)
        {
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(groupEvaluationId);
            if (groupEvaluation == null)
                throw new ApplicationException("Group evaluation not found");

            var student = await _unitOfWork.Students.GetStudentByIdAsync(studentId);
            if (student == null)
                throw new ApplicationException("Student not found");

            var panel = await _unitOfWork.Panels.GetPanelByIdAsync(groupEvaluation.PanelId);
            if (panel == null)
                throw new ApplicationException("Panel not found");

            var studentEvaluation = groupEvaluation.StudentEvaluations
                .FirstOrDefault(se => se.StudentId == studentId);

            var allScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(studentEvaluation?.Id ?? 0);
            if (allScores == null || !allScores.Any())
            {
                return new EvaluationStatisticsDto
                {
                    StudentId = studentId,
                    StudentName = student?.FullName ?? "Unknown",
                    TotalEvaluators = panel?.Members?.Count ?? 0,
                    CompletedEvaluators = 0,
                    RemainingEvaluators = panel?.Members?.Count ?? 0,
                    IsComplete = false,
                    FinalScore = 0,
                    EvaluatorSummaries = new List<EvaluatorSummaryDto>()
                };
            }

            var evaluatorIds = allScores.Select(s => s.EvaluatorId).Distinct().ToList();

            var evaluatorSummaries = new List<EvaluatorSummaryDto>();

            foreach (var member in panel.Members)
            {
                var teacherId = member.TeacherId;
                var teacher = member.Teacher;
                var hasEvaluated = evaluatorIds.Contains(teacherId);

                int? averageScore = null;
                DateTime? evaluatedAt = null;

                if (hasEvaluated)
                {
                    var teacherScores = allScores.Where(s => s.EvaluatorId == teacherId).ToList();
                    if (teacherScores.Any())
                    {
                        var event_ = await _unitOfWork.Evaluations.GetEventByIdAsync(groupEvaluation.EventId);
                        var rubricId = event_?.RubricId;
                        if (!rubricId.HasValue) continue;

                        var rubric = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(rubricId.Value);
                        if (rubric == null) continue;

                        double evaluatorWeightedTotal = 0;
                        double totalWeight = 0;

                        foreach (var category in rubric.Categories)
                        {
                            var score = teacherScores.FirstOrDefault(s => s.CategoryId == category.Id);
                            if (score != null)
                            {
                                double scorePercentage = (double)score.Score / category.MaxScore;
                                evaluatorWeightedTotal += scorePercentage * category.Weight;
                                totalWeight += category.Weight;
                            }
                        }

                        double evaluatorScore = 0;
                        if (totalWeight > 0)
                        {
                            double weightedPercentage = evaluatorWeightedTotal / totalWeight;
                            evaluatorScore = weightedPercentage * 100;
                        }

                        averageScore = (int)Math.Round(evaluatorScore * event_.TotalMarks / 100);
                        evaluatedAt = teacherScores.Max(s => s.EvaluatedAt);
                    }
                }

                evaluatorSummaries.Add(new EvaluatorSummaryDto
                {
                    EvaluatorId = teacherId,
                    EvaluatorName = teacher.FullName,
                    HasEvaluated = hasEvaluated,
                    AverageScore = averageScore,
                    EvaluatedAt = evaluatedAt
                });
            }

            return new EvaluationStatisticsDto
            {
                StudentEvaluationId = studentEvaluation.Id,
                StudentId = studentId,
                StudentName = student.FullName,
                TotalEvaluators = panel.Members.Count,
                CompletedEvaluators = evaluatorIds.Count,
                RemainingEvaluators = panel.Members.Count - evaluatorIds.Count,
                IsComplete = studentEvaluation.IsComplete,
                FinalScore = studentEvaluation.ObtainedMarks,
                EvaluatorSummaries = evaluatorSummaries
            };
        }

        public async Task<StudentEvaluation> GetStudentEvaluationByGroupEvaluationAndStudentIdAsync(int groupEvaluationId, int studentId)
        {
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(groupEvaluationId);
            if (groupEvaluation == null)
                throw new ApplicationException($"Group evaluation with ID {groupEvaluationId} not found");

            var studentEvaluation = groupEvaluation.StudentEvaluations?
                .FirstOrDefault(se => se.StudentId == studentId);

            return studentEvaluation ?? throw new ApplicationException($"Student evaluation not found for student ID {studentId} in group evaluation {groupEvaluationId}");
        }

        public async Task<EnhancedStudentEvaluationDto> GetTeacherEvaluationForStudentAsync(int teacherId, int groupEvaluationId, int studentId)
        {
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(groupEvaluationId);
            if (groupEvaluation == null)
                throw new ApplicationException($"Group evaluation with ID {groupEvaluationId} not found");

            var studentEvaluation = groupEvaluation.StudentEvaluations?
                .FirstOrDefault(se => se.StudentId == studentId);

            var student = await _unitOfWork.Students.GetStudentByIdAsync(studentId);
            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(groupEvaluation.EventId);

            var resultDto = new EnhancedStudentEvaluationDto
            {
                StudentId = studentId,
                StudentName = student?.FullName ?? "Unknown",
                EventName = evaluationEvent?.Name,
                TotalMarks = evaluationEvent?.TotalMarks ?? 0,
                EventDate = evaluationEvent?.Date,
                CategoryScores = new List<CategoryScoreDetailDto>(),
                Evaluators = new List<EvaluatorDto>(),
                IsComplete = false
            };

            if (studentEvaluation == null)
            {
                return resultDto;
            }

            var teacherScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAndEvaluatorIdAsync(
                studentEvaluation.Id, teacherId);

            bool hasTeacherEvaluated = teacherScores != null && teacherScores.Any();

            if (!hasTeacherEvaluated)
            {
                resultDto.Id = studentEvaluation.Id;
                resultDto.IsComplete = studentEvaluation.IsComplete;
                return resultDto;
            }

            if (evaluationEvent?.RubricId.HasValue == true)
            {
                var rubric = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(evaluationEvent.RubricId.Value);
                if (rubric != null)
                {
                    foreach (var category in rubric.Categories)
                    {
                        var score = teacherScores != null ? teacherScores.FirstOrDefault(s => s.CategoryId == category.Id) : null;
                        if (score != null)
                        {
                            resultDto.CategoryScores.Add(new CategoryScoreDetailDto
                            {
                                CategoryId = category.Id,
                                CategoryName = category.Name,
                                CategoryWeight = category.Weight,
                                Score = score.Score,
                                MaxScore = category.MaxScore,
                                Feedback = score.Feedback,
                                EvaluatorDetails = new List<CategoryEvaluatorDetailDto>
                                {
                                    new CategoryEvaluatorDetailDto
                                    {
                                        EvaluatorId = teacherId,
                                        Score = score.Score,
                                        Feedback = score.Feedback,
                                        EvaluatedAt = score.EvaluatedAt
                                    }
                                }
                            });
                        }
                        else
                        {
                            resultDto.CategoryScores.Add(new CategoryScoreDetailDto
                            {
                                CategoryId = category.Id,
                                CategoryName = category.Name,
                                CategoryWeight = category.Weight,
                                Score = 0,
                                MaxScore = category.MaxScore,
                                Feedback = "",
                                EvaluatorDetails = new List<CategoryEvaluatorDetailDto>()
                            });
                        }
                    }
                }
            }

            var teacherFeedback = teacherScores != null ?
                teacherScores.Where(s => !string.IsNullOrEmpty(s.Feedback))
                    .Select(s => s.Feedback)
                    .FirstOrDefault() : null;

            resultDto.Id = studentEvaluation.Id;
            resultDto.ObtainedMarks = studentEvaluation.ObtainedMarks;
            resultDto.Feedback = teacherFeedback ?? string.Empty;
            resultDto.EvaluatedAt = teacherScores.Any() ? teacherScores.Max(s => s.EvaluatedAt) : DateTime.MinValue;
            resultDto.IsComplete = studentEvaluation.IsComplete;

            return resultDto;
        }

        private async Task<StudentEvaluationDto> MapToStudentEvaluationDtoAsync(StudentEvaluation evaluation)
        {
            var student = await _unitOfWork.Students.GetStudentByIdAsync(evaluation.StudentId);
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(evaluation.GroupEvaluationId);

            return new StudentEvaluationDto
            {
                Id = evaluation.Id,
                StudentId = evaluation.StudentId,
                StudentName = student?.FullName ?? "Unknown",
                ObtainedMarks = evaluation.ObtainedMarks,
                Feedback = evaluation.Feedback,
                EvaluatedAt = evaluation.EvaluatedAt,
                IsComplete = evaluation.IsComplete,
                EventName = groupEvaluation?.Event?.Name,
                EventDate = groupEvaluation?.Event?.Date,
                TotalMarks = groupEvaluation?.Event?.TotalMarks,
                PercentageObtained = groupEvaluation?.Event?.TotalMarks > 0 
                    ? (evaluation.ObtainedMarks * 100M) / groupEvaluation.Event.TotalMarks 
                    : 0
            };
        }
    }
}

