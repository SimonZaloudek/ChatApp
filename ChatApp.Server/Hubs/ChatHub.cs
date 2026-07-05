using ChatApp.Core;
using ChatApp.Server.Data;
using ChatApp.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _db;

        public ChatHub(ChatDbContext db)
        {
            _db = db;
        }

        public async Task SendMessage(int userId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new HubException("Message content is required.");

            var user = await _db.Users.FindAsync(userId)
                ?? throw new HubException($"User {userId} does not exist.");

            var message = new Message
            {
                UserId = user.Id,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", new ChatMessage
            {
                Username = user.Username,
                Content = message.Content,
                SentAt = message.SentAt
            });
        }
    }
}
