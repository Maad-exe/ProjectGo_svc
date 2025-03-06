namespace backend.DTOs
{
    public class TeacherDetailsDto
    {
        public int Id { get; set; }
        public string fullName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string qualification { get; set; } = string.Empty;
        public string areaOfSpecialization { get; set; } = string.Empty;
        public string officeLocation { get; set; } = string.Empty;

        public int AssignedGroups { get; set; }

    }

}
