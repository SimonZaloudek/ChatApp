namespace ChatApp.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
