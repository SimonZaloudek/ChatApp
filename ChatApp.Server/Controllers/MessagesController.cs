using ChatApp.Core;
using ChatApp.Server.Data;
using ChatApp.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ChatDbContext _db;
        private readonly ChatMessageService _messages;

        public MessagesController(ChatDbContext db, ChatMessageService messages)
        {
            _db = db;
            _messages = messages;
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

            var sent = await _messages.SendAsync(request.UserId, request.Content);
            if (sent is null)
                return NotFound($"User {request.UserId} does not exist.");

            return sent;
        }
    }

    public record SendMessageRequest(int UserId, string Content);
}
