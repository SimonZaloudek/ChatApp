using ChatApp.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatMessageService _messages;

        public ChatHub(ChatMessageService messages)
        {
            _messages = messages;
        }

        public async Task SendMessage(int userId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new HubException("Message content is required.");

            var sent = await _messages.SendAsync(userId, content);
            if (sent is null)
                throw new HubException($"User {userId} does not exist.");
        }
    }
}
