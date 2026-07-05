using ChatApp.Core;
using ChatApp.Server.Data;
using ChatApp.Server.Hubs;
using ChatApp.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ChatDbContext _db;
        private readonly IHubContext<ChatHub> _hub;

        public MessagesController(ChatDbContext db, IHubContext<ChatHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        /// <summary>Returns the most recent messages, oldest first, ready to render.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessages([FromQuery] int count = 50)
        {
            count = Math.Clamp(count, 1, 200);

            var recent = await _db.Messages
                .OrderByDescending(m => m.SentAt)
                .ThenByDescending(m => m.Id)
                .Take(count)
                .Select(m => new ChatMessage
                {
                    Username = m.User!.Username,
                    Content = m.Content,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            recent.Reverse();
            return recent;
        }

        [HttpPost]
        public async Task<ActionResult<ChatMessage>> SendMessage(SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Message content is required.");

            var user = await _db.Users.FindAsync(request.UserId);
            if (user is null)
                return NotFound($"User {request.UserId} does not exist.");

            var message = new Message
            {
                UserId = user.Id,
                Content = request.Content,
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

    public record SendMessageRequest(int UserId, string Content);
}
