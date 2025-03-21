namespace backend.Core.Entities.PanelManagement
{
    public class Panel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Using local time as per your preference
        public ICollection<PanelMember> Members { get; set; } = new List<PanelMember>();
    }
}
