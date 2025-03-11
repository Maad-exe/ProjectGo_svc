using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class CreateGroupDto
    {
        [Required]
        public string GroupName { get; set; } = string.Empty;

        [Required]
        public List<string> MemberEmails { get; set; } = new List<string>();
    }

    public class StudentDetailsDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EnrollmentNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsCreator { get; set; }
    }

    
    public class GroupDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<StudentDetailsDto> Members { get; set; } = new List<StudentDetailsDto>();
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public string SupervisionStatus { get; set; } = "None";
    }

}