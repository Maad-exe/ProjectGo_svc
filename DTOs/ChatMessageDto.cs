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
        public bool IsRead { get; set; } // Legacy field - true if read by all
        public List<MessageReadStatusDto> ReadBy { get; set; } = new List<MessageReadStatusDto>(); // New property
        public int TotalReadCount { get; set; } // New property
        public bool IsReadByCurrentUser { get; set; } // New property
    }

    public class SendMessageDto
    {
        public int GroupId { get; set; }
        public string Content { get; set; }
    }

    // New DTO for message read status
    public class MessageReadStatusDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime ReadAt { get; set; }
    }

    // New DTO for group info
    public class GroupInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int MemberCount { get; set; }
    }
}