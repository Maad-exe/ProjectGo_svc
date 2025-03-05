namespace backend.Core.Entities
{
    public class Teacher : User
    {
        public string Qualification { get; set; } = string.Empty;
        public string AreaOfSpecialization { get; set; } = string.Empty;
        public string OfficeLocation { get; set; } = string.Empty;
        public int AssignedGroups { get; set; }
    }
}
