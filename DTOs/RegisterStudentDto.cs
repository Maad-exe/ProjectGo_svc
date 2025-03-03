namespace backend.DTOs
{
    public class RegisterStudentDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string EnrollmentNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }
}
