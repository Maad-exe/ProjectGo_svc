using System.ComponentModel.DataAnnotations;
using backend.Core.Enums;
namespace backend.Core.Entities
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public GroupSupervisionStatus SupervisionStatus { get; set; } = GroupSupervisionStatus.None;
    
}

    public class GroupMember
    {
        [Key]
        public int Id { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public bool IsCreator { get; set; } = false;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}