using ChatApp.Core;
using ChatApp.Server.Data;
using ChatApp.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ChatDbContext _db;

        public MessagesController(ChatDbContext db)
        {
            _db = db;
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

            return new ChatMessage
            {
                Username = user.Username,
                Content = message.Content,
                SentAt = message.SentAt
            };
        }
    }

    public record SendMessageRequest(int UserId, string Content);
}
