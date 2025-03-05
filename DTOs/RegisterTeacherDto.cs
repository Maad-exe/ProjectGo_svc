namespace backend.DTOs
{
    public class RegisterTeacherDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string AreaOfSpecialization { get; set; } = string.Empty;
        public string OfficeLocation { get; set; } = string.Empty;

       
    }
}

