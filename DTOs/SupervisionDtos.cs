namespace backend.DTOs
{
  
    public class SupervisionRequestDto
    {
        public int GroupId { get; set; }
        public int TeacherId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SupervisionResponseDto
    {
        public int GroupId { get; set; }
        public bool IsApproved { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class TeacherSupervisionRequestDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public List<StudentDetailsDto> GroupMembers { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class StudentSupervisionStatusDto
    {
        public bool IsInSupervisedGroup { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;
    }

}
