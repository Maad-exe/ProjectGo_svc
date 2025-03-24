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
                CreatedAt = createdEvent.CreatedAt
            };
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
                CreatedAt = evaluationEvent.CreatedAt
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
                CreatedAt = e.CreatedAt
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
                CreatedAt = evaluationEvent.CreatedAt
            };
        }

        public async Task DeleteEventAsync(int eventId)
        {
            await _unitOfWork.Evaluations.DeleteEventAsync(eventId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<GroupEvaluationDto> AssignPanelToGroupAsync(AssignPanelDto assignDto)
        {
            // Check if panel exists
            var panel = await _unitOfWork.Panels.GetPanelByIdAsync(assignDto.PanelId);
            if (panel == null)
                throw new ApplicationException($"Panel with ID {assignDto.PanelId} not found");

            // Check if group exists
            var group = await _unitOfWork.Groups.GetGroupByIdAsync(assignDto.GroupId);
            if (group == null)
                throw new ApplicationException($"Group with ID {assignDto.GroupId} not found");

            // Check if event exists
            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(assignDto.EventId);
            if (evaluationEvent == null)
                throw new ApplicationException($"Event with ID {assignDto.EventId} not found");

            // Check if the group has a supervisor
            if (!group.TeacherId.HasValue)
                throw new ApplicationException("Cannot assign panel to a group without a supervisor");

            // Validate that supervisor is not in the panel (key business rule)
            var supervisorId = group.TeacherId.Value;
            var supervisorInPanel = await _unitOfWork.Panels.IsTeacherInPanelAsync(assignDto.PanelId, supervisorId);
            if (supervisorInPanel)
                throw new ApplicationException("Cannot assign a panel that includes the group's supervisor");

            // Check if the group is already evaluated for this event
            var existingEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationsByGroupIdAsync(assignDto.GroupId);
            if (existingEvaluation.Any(ge => ge.EventId == assignDto.EventId))
                throw new ApplicationException($"Group is already assigned for evaluation for this event");

            // Create the group evaluation
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

            // Return the DTO with details
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
            // Get all panels that the teacher is a part of
            var panels = await _unitOfWork.Panels.GetPanelsByTeacherIdAsync(teacherId);

            if (!panels.Any())
                return new List<GroupEvaluationDto>();

            var result = new List<GroupEvaluationDto>();

            // For each panel, get its evaluations
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
            // Check if group evaluation exists
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(evaluationDto.GroupEvaluationId);
            if (groupEvaluation == null)
                throw new ApplicationException($"Group evaluation with ID {evaluationDto.GroupEvaluationId} not found");

            // Check if student exists and is part of the group
            var student = await _unitOfWork.Students.GetStudentByIdAsync(evaluationDto.StudentId);
            if (student == null)
                throw new ApplicationException($"Student with ID {evaluationDto.StudentId} not found");

            var studentInGroup = groupEvaluation.Group.Members.Any(m => m.StudentId == evaluationDto.StudentId);
            if (!studentInGroup)
                throw new ApplicationException("Student is not a member of the group being evaluated");

            // Validate marks
            if (evaluationDto.ObtainedMarks < 0 || evaluationDto.ObtainedMarks > groupEvaluation.Event.TotalMarks)
                throw new ApplicationException($"Marks must be between 0 and {groupEvaluation.Event.TotalMarks}");

            // Check if student has already been evaluated
            var existingEvaluation = groupEvaluation.StudentEvaluations
                .FirstOrDefault(se => se.StudentId == evaluationDto.StudentId);

            if (existingEvaluation != null)
                throw new ApplicationException("Student has already been evaluated for this event");

            // Create the student evaluation
            var studentEvaluation = new StudentEvaluation
            {
                GroupEvaluationId = evaluationDto.GroupEvaluationId,
                StudentId = evaluationDto.StudentId,
                ObtainedMarks = evaluationDto.ObtainedMarks ?? 0,
                Feedback = evaluationDto.Feedback,
                EvaluatedAt = DateTime.Now
            };

            var createdEvaluation = await _unitOfWork.Evaluations.EvaluateStudentAsync(studentEvaluation);

            // Check if all students in the group have been evaluated
            var groupMembers = groupEvaluation.Group.Members.Count();
            var evaluatedStudents = groupEvaluation.StudentEvaluations.Count() + 1; // +1 for the one we just added

            if (groupMembers == evaluatedStudents)
            {
                // Mark the group evaluation as completed
                groupEvaluation.IsCompleted = true;
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return new StudentEvaluationDto
            {
                Id = createdEvaluation.Id,
                StudentId = student.Id,
                StudentName = student.FullName,
                ObtainedMarks = createdEvaluation.ObtainedMarks,
                Feedback = createdEvaluation.Feedback,
                EvaluatedAt = createdEvaluation.EvaluatedAt
            };
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
                StudentName = string.Empty, // Student name is not needed as we already know the student
                ObtainedMarks = e.ObtainedMarks,
                Feedback = e.Feedback,
                EvaluatedAt = e.EvaluatedAt,
                // Additional info about the evaluation
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
            // Get groups supervised by this teacher
            var supervisedGroups = await _unitOfWork.Groups.GetGroupsByTeacherIdAsync(teacherId);

            if (!supervisedGroups.Any())
                return new List<GroupPerformanceDto>();

            var result = new List<GroupPerformanceDto>();

            foreach (var group in supervisedGroups)
            {
                // Get all evaluations for this group
                var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByGroupIdAsync(group.Id);

                // Calculate performance metrics for this group
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
                    var studentEvaluations = await _unitOfWork.Evaluations.GetStudentEvaluationsByGroupEvaluationIdAsync(evaluation.Id);

                    // Skip if no evaluations yet
                    if (!studentEvaluations.Any())
                        continue;

                    // Calculate average score
                    var totalMarks = studentEvaluations.Sum(se => se.ObtainedMarks);
                    var averageMarks = totalMarks / (double)studentEvaluations.Count;
                    var percentage = (averageMarks * 100) / evaluationEvent.TotalMarks;

                    // Add to total for group average calculation
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

                    // Add individual student performances
                    foreach (var se in studentEvaluations)
                    {
                        var student = await _unitOfWork.Students.GetStudentByIdAsync(se.StudentId);
                        eventPerformance.StudentPerformances.Add(new StudentPerformanceDto
                        {
                            StudentId = se.StudentId,
                            StudentName = student?.FullName ?? "Unknown",
                            ObtainedMarks = se.ObtainedMarks,
                            Percentage = (se.ObtainedMarks * 100.0) / evaluationEvent.TotalMarks,
                            Feedback = se.Feedback
                        });
                    }
                    groupPerformance.Events.Add(eventPerformance);
                }

                // Calculate overall average performance for the group
                groupPerformance.AveragePerformance = evaluationCount > 0 ? totalPercentage / evaluationCount : 0;

                result.Add(groupPerformance);
            }

            return result;
        }

        public async Task<TeacherDashboardDto> GetTeacherDashboardAsync(int teacherId)
        {
            // Get supervised groups count
            var supervisedGroups = await _unitOfWork.Groups.GetGroupsByTeacherIdAsync(teacherId);

            // Get panel memberships count
            var panels = await _unitOfWork.Panels.GetPanelsByTeacherIdAsync(teacherId);

            // Get total evaluations count for panels the teacher is part of
            int totalEvaluationsCount = 0;
            foreach (var panel in panels)
            {
                var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByPanelIdAsync(panel.Id);
                totalEvaluationsCount += evaluations.Count;
            }

            // Get detailed performance for supervised groups
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

            // Update properties
            existingEvaluation.ScheduledDate = updatedEvaluation.ScheduledDate;
            existingEvaluation.IsCompleted = updatedEvaluation.IsCompleted;
            existingEvaluation.Comments = updatedEvaluation.Comments;

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            return await MapGroupEvaluationToDto(existingEvaluation);
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            // Get counts of various entities
            var panels = await _unitOfWork.Panels.GetAllPanelsAsync();
            var events = await _unitOfWork.Evaluations.GetAllEventsAsync();
            var allGroups = await _unitOfWork.Groups.GetAllGroupsAsync();
            var supervisedGroups = allGroups.Where(g => g.TeacherId.HasValue).ToList();

            // Get all evaluations
            var allEvaluations = new List<GroupEvaluation>();
            foreach (var e in events)
            {
                var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByEventIdAsync(e.Id);
                allEvaluations.AddRange(evaluations);
            }

            // Get event statistics
            var eventStats = new List<EventStatisticsDto>();
            foreach (var evt in events)
            {
                var evaluations = await _unitOfWork.Evaluations.GetGroupEvaluationsByEventIdAsync(evt.Id);

                double averagePerformance = 0;
                int evaluatedGroupsCount = 0;

                foreach (var eval in evaluations)
                {
                    var studentEvaluations = await _unitOfWork.Evaluations.GetStudentEvaluationsByGroupEvaluationIdAsync(eval.Id);
                    if (studentEvaluations.Any())
                    {
                        double groupAvg = studentEvaluations.Average(se =>
                            (se.ObtainedMarks * 100.0) / evt.TotalMarks);
                        averagePerformance += groupAvg;
                        evaluatedGroupsCount++;
                    }
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
            // Validate weights add up to 1.0 (100%)
            double totalWeight = rubricDto.Categories.Sum(c => c.Weight);
            if (Math.Abs(totalWeight - 1.0) > 0.01) // Allow small rounding errors
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

            // Validate weights
            double totalWeight = rubricDto.Categories.Sum(c => c.Weight);
            if (Math.Abs(totalWeight - 1.0) > 0.01)
            {
                throw new ApplicationException("Category weights must sum to 1.0 (100%)");
            }

            rubric.Name = rubricDto.Name;
            rubric.Description = rubricDto.Description;
            rubric.IsActive = rubricDto.IsActive;

            // Update existing categories and add new ones
            foreach (var categoryDto in rubricDto.Categories)
            {
                var category = rubric.Categories.FirstOrDefault(c => c.Id == categoryDto.Id);

                if (category != null)
                {
                    // Update existing category
                    category.Name = categoryDto.Name;
                    category.Description = categoryDto.Description;
                    category.Weight = categoryDto.Weight;
                    category.MaxScore = categoryDto.MaxScore;
                }
                else
                {
                    // Add new category
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

            // Remove categories that are not in the update DTO
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
            // Check if rubric is used in any events
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
            // Check if group evaluation exists
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(evaluationDto.GroupEvaluationId);
            if (groupEvaluation == null)
                throw new ApplicationException($"Group evaluation with ID {evaluationDto.GroupEvaluationId} not found");

            // Get the event and check if it has a rubric
            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(groupEvaluation.EventId);
            if (evaluationEvent == null)
                throw new ApplicationException("Evaluation event not found");

            // Check if student exists and is part of the group
            var student = await _unitOfWork.Students.GetStudentByIdAsync(evaluationDto.StudentId);
            if (student == null)
                throw new ApplicationException($"Student with ID {evaluationDto.StudentId} not found");

            var studentInGroup = groupEvaluation.Group.Members.Any(m => m.StudentId == evaluationDto.StudentId);
            if (!studentInGroup)
                throw new ApplicationException("Student is not a member of the group being evaluated");

            // Get or create student evaluation
            var studentEvaluation = groupEvaluation.StudentEvaluations
                .FirstOrDefault(se => se.StudentId == evaluationDto.StudentId);

            if (studentEvaluation == null)
            {
                // Create new evaluation
                studentEvaluation = new StudentEvaluation
                {
                    GroupEvaluationId = evaluationDto.GroupEvaluationId,
                    StudentId = evaluationDto.StudentId,
                    RubricId = evaluationEvent.RubricId,
                    ObtainedMarks = 0, // Will calculate later
                    Feedback = evaluationDto.Feedback ?? string.Empty,
                    EvaluatedAt = DateTime.Now,
                    IsComplete = false
                };

                var createdEvaluation = await _unitOfWork.Evaluations.EvaluateStudentAsync(studentEvaluation);
                await _unitOfWork.SaveChangesAsync(); // Save to get ID

                studentEvaluation = createdEvaluation; // Use the created entity with ID
            }

            // Check if teacher is allowed to evaluate
            bool hasEvaluated = await _unitOfWork.Evaluations.HasTeacherEvaluatedStudentAsync(teacherId, studentEvaluation.Id);
            if (hasEvaluated && evaluationEvent.RubricId.HasValue)
            {
                throw new ApplicationException("You have already evaluated this student for this event");
            }

            // Add teacher as evaluator
            await _unitOfWork.Evaluations.AddEvaluatorToStudentEvaluationAsync(studentEvaluation.Id, teacherId);

            // Handle rubric-based evaluation
            if (evaluationEvent.RubricId.HasValue && evaluationDto.CategoryScores != null && evaluationDto.CategoryScores.Any())
            {
                var rubric = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(evaluationEvent.RubricId.Value);
                if (rubric == null)
                    throw new ApplicationException("Rubric not found for this evaluation event");

                // Evaluate each category
                foreach (var categoryScore in evaluationDto.CategoryScores)
                {
                    var category = rubric.Categories.FirstOrDefault(c => c.Id == categoryScore.CategoryId);
                    if (category == null)
                        throw new ApplicationException($"Category with ID {categoryScore.CategoryId} not found in rubric");

                    // Validate score
                    if (categoryScore.Score < 0 || categoryScore.Score > category.MaxScore)
                        throw new ApplicationException($"Score for {category.Name} must be between 0 and {category.MaxScore}");

                    // Save category score
                    var score = new StudentCategoryScore
                    {
                        StudentEvaluationId = studentEvaluation.Id,
                        CategoryId = categoryScore.CategoryId,
                        Score = categoryScore.Score,
                        Feedback = categoryScore.Feedback,
                        EvaluatorId = teacherId,
                        EvaluatedAt = DateTime.Now
                    };

                    await _unitOfWork.Rubrics.AddCategoryScoreAsync(score);
                }

                // Calculate total marks based on category scores
                await CalculateObtainedMarksFromCategoryScores(studentEvaluation.Id, evaluationEvent);
            }
            else if (evaluationDto.ObtainedMarks.HasValue)
            {
                // Legacy evaluation with single score
                if (evaluationDto.ObtainedMarks < 0 || evaluationDto.ObtainedMarks > evaluationEvent.TotalMarks)
                    throw new ApplicationException($"Marks must be between 0 and {evaluationEvent.TotalMarks}");

                studentEvaluation.ObtainedMarks = evaluationDto.ObtainedMarks.Value;
                studentEvaluation.Feedback = evaluationDto.Feedback ?? studentEvaluation.Feedback;
            }
            else
            {
                throw new ApplicationException("Either category scores or total marks must be provided");
            }

            // Check if all required evaluators have evaluated (for rubric-based evaluation)
            var evaluators = await _unitOfWork.Evaluations.GetEvaluatorsByStudentEvaluationIdAsync(studentEvaluation.Id);
            bool allCategoriesEvaluated = true;

            if (evaluationEvent.RubricId.HasValue)
            {
                var rubric = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(evaluationEvent.RubricId.Value);
                var categoryScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(studentEvaluation.Id);

                // Check if all categories have been evaluated
                allCategoriesEvaluated = rubric.Categories.All(c =>
                    categoryScores.Any(s => s.CategoryId == c.Id));
            }

            // Mark as complete if all evaluators have evaluated or if using legacy evaluation
            studentEvaluation.IsComplete = evaluationEvent.RubricId.HasValue
                ? allCategoriesEvaluated
                : true;

            // Check if all students in the group have been evaluated
            if (studentEvaluation.IsComplete)
            {
                var groupMembers = groupEvaluation.Group.Members.Count();
                var completedEvaluations = groupEvaluation.StudentEvaluations.Count(se => se.IsComplete);

                if (groupMembers == completedEvaluations)
                {
                    groupEvaluation.IsCompleted = true;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Return enhanced DTO with category scores
            return await MapToEnhancedStudentEvaluationDto(studentEvaluation);
        }

        private async Task CalculateObtainedMarksFromCategoryScores(int studentEvaluationId, EvaluationEvent evaluationEvent)
        {
            if (!evaluationEvent.RubricId.HasValue)
                return;

            var rubric = await _unitOfWork.Rubrics.GetRubricWithCategoriesAsync(evaluationEvent.RubricId.Value);
            if (rubric == null)
                return;

            var categoryScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(studentEvaluationId);
            if (!categoryScores.Any())
                return;

            double weightedTotal = 0;

            foreach (var category in rubric.Categories)
            {
                var scores = categoryScores.Where(s => s.CategoryId == category.Id).ToList();
                if (scores.Any())
                {
                    // Average scores if multiple evaluators scored the same category
                    double averageScore = scores.Average(s => s.Score);

                    // Calculate weighted score
                    weightedTotal += (averageScore / category.MaxScore) * category.Weight;
                }
            }

            // Scale to event total marks
            int totalMarks = (int)Math.Round(weightedTotal * evaluationEvent.TotalMarks);

            var studentEvaluation = (await _unitOfWork.Evaluations.GetStudentEvaluationsByGroupEvaluationIdAsync(studentEvaluationId))
        .FirstOrDefault(se => se.Id == studentEvaluationId);

            if (studentEvaluation != null)
            {
                studentEvaluation.ObtainedMarks = totalMarks;
            }
        }

        private async Task<EnhancedStudentEvaluationDto> MapToEnhancedStudentEvaluationDto(StudentEvaluation evaluation)
        {
            var student = await _unitOfWork.Students.GetStudentByIdAsync(evaluation.StudentId);
            var groupEvaluation = await _unitOfWork.Evaluations.GetGroupEvaluationByIdAsync(evaluation.GroupEvaluationId);
            var evaluationEvent = await _unitOfWork.Evaluations.GetEventByIdAsync(groupEvaluation.EventId);

            // Safely handle null values
            int totalMarks = evaluationEvent?.TotalMarks ?? 0;
            double eventWeight = evaluationEvent?.Weight ?? 1.0;

            // Calculate percentage with null checks
            decimal? percentageObtained = totalMarks > 0
                ? (decimal?)((evaluation.ObtainedMarks * 100.0) / totalMarks)
                : 0;

            // Calculate weighted score
            double weightedScore = totalMarks > 0
                ? (evaluation.ObtainedMarks * 100.0 * eventWeight) / totalMarks
                : 0;

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
                Evaluators = new List<EvaluatorDto>()
            };

            // Add category scores
            if (evaluation.RubricId.HasValue)
            {
                var categoryScores = await _unitOfWork.Rubrics.GetScoresByStudentEvaluationIdAsync(evaluation.Id);
                foreach (var score in categoryScores)
                {
                    var evaluator = await _unitOfWork.Teachers.GetTeacherByIdAsync(score.EvaluatorId);

                    dto.CategoryScores.Add(new CategoryScoreDetailDto
                    {
                        CategoryId = score.CategoryId,
                        CategoryName = score.Category.Name,
                        CategoryWeight = score.Category.Weight,
                        Score = score.Score,
                        MaxScore = score.Category.MaxScore,
                        WeightedScore = (score.Score * score.Category.Weight) / score.Category.MaxScore * 100,
                        Feedback = score.Feedback,
                        Evaluator = new EvaluatorDto
                        {
                            Id = evaluator?.Id ?? 0,
                            Name = evaluator?.FullName ?? "Unknown"
                        }
                    });
                }
            }

            // Add evaluators
            var evaluators = await _unitOfWork.Evaluations.GetEvaluatorsByStudentEvaluationIdAsync(evaluation.Id);
            foreach (var evaluator in evaluators)
            {
                dto.Evaluators.Add(new EvaluatorDto
                {
                    Id = evaluator.Id,
                    Name = evaluator.FullName
                });
            }

            return dto;
        }

        private async Task<EvaluationRubricDto> MapRubricToDto(EvaluationRubric rubric)
        {
            // Load categories if not already loaded
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
            // Get all completed evaluations
            var allEvaluations = await _unitOfWork.Evaluations.GetAllStudentEvaluationsForNormalizationAsync();

            // Group by student
            var studentGroups = allEvaluations
                .GroupBy(se => se.StudentId)
                .ToList();

            if (!studentGroups.Any())
                return new List<NormalizedGradeDto>();

            // Calculate raw scores for each student
            var rawScores = new List<(int StudentId, string StudentName, double Score)>();

            foreach (var group in studentGroups)
            {
                var studentId = group.Key;
                var student = await _unitOfWork.Students.GetStudentByIdAsync(studentId);
                if (student == null) continue;

                double finalGrade = await CalculateFinalGradeAsync(studentId);
                rawScores.Add((studentId, student.FullName, finalGrade));
            }

            // Calculate statistics for normalization
            double mean = rawScores.Average(s => s.Score);
            double stdDev = Math.Sqrt(rawScores.Average(s => Math.Pow(s.Score - mean, 2)));

            // Normalize scores (z-score * 10 + 70 to center around 70%)
            var normalizedScores = rawScores
                .Select(s => new NormalizedGradeDto
                {
                    StudentId = s.StudentId,
                    StudentName = s.StudentName,
                    RawGrade = s.Score,
                    NormalizedGrade = stdDev > 0
                        ? Math.Min(100, Math.Max(0, ((s.Score - mean) / stdDev) * 10 + 70))
                        : s.Score // If no variation, keep raw score
                })
                .OrderByDescending(s => s.NormalizedGrade)
                .ToList();

            return normalizedScores;
        }


    }
}

