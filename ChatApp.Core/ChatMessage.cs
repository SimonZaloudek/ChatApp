namespace ChatApp.Core
{
    public class ChatMessage
    {
        public string Username { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime SentAt { get; set; }
    }
}
