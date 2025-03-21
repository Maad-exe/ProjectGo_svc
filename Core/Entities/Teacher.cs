using backend.Core.Entities.PanelManagement;

namespace backend.Core.Entities
{
    public class Teacher : User
    {
        public string Qualification { get; set; } = string.Empty;
        public string AreaOfSpecialization { get; set; } = string.Empty;
        public string OfficeLocation { get; set; } = string.Empty;
        public int AssignedGroups { get; set; }

        public ICollection<PanelMember> PanelMemberships { get; set; } = new List<PanelMember>();
    }
}
