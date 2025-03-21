namespace backend.Core.Entities.PanelManagement
{
    public class PanelMember
    {
        public int Id { get; set; }
        public int PanelId { get; set; }
        public Panel Panel { get; set; } = null!;
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;
        public bool IsHead { get; set; } = false;
    }
}
