using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceApp.Data;
using ServiceApp.Models;

namespace ServiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly AppDbContext _db;
        public ChatController(AppDbContext db) => _db = db;

        [HttpGet("{userId}/{otherUserId}")]
        public async Task<IActionResult> GetChatHistory(Guid userId, Guid otherUserId)
        {
            var messages = await _db.ChatMessages
                .Where(m => (m.SenderId == userId.ToString() && m.ReceiverId == otherUserId.ToString()) ||
                            (m.SenderId == otherUserId.ToString() && m.ReceiverId == userId.ToString()))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            return Ok(messages);
        }
        [HttpPost]
        public async Task<IActionResult> SaveMessage([FromBody] ChatMessage message)
        {
            if (string.IsNullOrEmpty(message.Message))
                return BadRequest("Message cannot be empty.");

            message.Timestamp = DateTime.Now;
            _db.ChatMessages.Add(message);
            await _db.SaveChangesAsync();

            return Ok(message);
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] List<IFormFile>? file)
        {
            string? imagePath = null;

            if (file != null && file.Any())
            {
                var uploadDir = Path.Combine("wwwroot", "chat");
                Directory.CreateDirectory(uploadDir);

                var image = file.First();
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                var savePath = Path.Combine(uploadDir, fileName);

                using var stream = new FileStream(savePath, FileMode.Create);
                await image.CopyToAsync(stream);

                imagePath = $"/chat/{fileName}";
            }

            return Ok(new { url = imagePath });
        }

    }
}
