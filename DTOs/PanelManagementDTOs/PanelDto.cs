namespace backend.DTOs.PanelManagementDTOs
{
    public class PanelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<PanelMemberDto> Members { get; set; } = new List<PanelMemberDto>();
    }

    public class PanelMemberDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public bool IsHead { get; set; }
    }

    public class CreatePanelDto
    {
        public string Name { get; set; } = string.Empty;
        public List<int> TeacherIds { get; set; } = new List<int>();
    }

    public class UpdatePanelDto
    {
        public string Name { get; set; } = string.Empty;
        public List<int> TeacherIds { get; set; } = new List<int>();
    }
}
