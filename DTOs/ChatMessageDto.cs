namespace backend.DTOs
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderRole { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }

    public class SendMessageDto
    {
       // public int GroupId { get; set; }
        public string Content { get; set; }
    }


}
