namespace backend.Core.Entities
{
    public class MessageReadStatus
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public DateTime ReadAt { get; set; }

        // Navigation properties
        public ChatMessage Message { get; set; }
        public User User { get; set; }
    }
}
