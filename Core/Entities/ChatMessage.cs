namespace backend.Core.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }

        // Navigation properties
        public Group Group { get; set; }
        public User Sender { get; set; }
    }
}
