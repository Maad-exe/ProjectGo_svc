
namespace backend.Core.Entities

{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsRead { get; set; }  // Keep for backward compatibility

        // Navigation properties
        public Group Group { get; set; }
        public User Sender { get; set; }

        // New navigation property
        public ICollection<MessageReadStatus> ReadStatuses { get; set; } = new List<MessageReadStatus>();
    }
}
