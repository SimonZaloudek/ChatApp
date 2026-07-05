using ChatApp.Core;
using ChatApp.Server.Data;
using ChatApp.Server.Hubs;
using ChatApp.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Services
{
    /// <summary>
    /// The single place where a chat message is persisted and broadcast,
    /// shared by the REST endpoint and the SignalR hub.
    /// </summary>
    public class ChatMessageService
    {
        private readonly ChatDbContext _db;
        private readonly IHubContext<ChatHub> _hub;

        public ChatMessageService(ChatDbContext db, IHubContext<ChatHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        /// <summary>Saves the message and pushes it to all connected clients. Returns null if the user doesn't exist.</summary>
        public async Task<ChatMessage?> SendAsync(int userId, string content)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null)
                return null;

            var message = new Message
            {
                UserId = user.Id,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            var dto = new ChatMessage
            {
                Username = user.Username,
                Content = message.Content,
                SentAt = message.SentAt
            };

            await _hub.Clients.All.SendAsync("ReceiveMessage", dto);
            return dto;
        }
    }
}
