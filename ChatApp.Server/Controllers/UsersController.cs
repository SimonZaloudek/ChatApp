using ChatApp.Server.Data;
using ChatApp.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ChatDbContext _db;

        public UsersController(ChatDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _db.Users.OrderBy(u => u.Username).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user is null)
                return NotFound();

            return user;
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
        {
            var username = request.Username.Trim();
            if (username.Length == 0)
                return BadRequest("Username is required.");

            var exists = await _db.Users.AnyAsync(u => u.Username == username);
            if (exists)
                return Conflict($"Username '{username}' is already taken.");

            var user = new User
            {
                Username = username,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
    }

    public record CreateUserRequest(string Username);
}
