﻿using backend.Core.Entities;

namespace backend.Infrastructure.Repositories.Contracts
{
    public interface ITeacherRepository
    {
        Task<Teacher?> GetUserByEmailAsync(string email);
        Task AddTeacherAsync(Teacher teacher);
        Task<List<Teacher>> GetAllTeachersAsync();

        Task<Teacher?> GetTeacherByIdAsync(int teacherId); 
        Task IncrementAssignedGroupsAsync(int teacherId);
    }
}
