using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceApp.Data;
using ServiceApp.Models;
using ServiceApp.Models.DTOs;
using ServiceApp.Models.Enums;
using System.Security.Claims;

namespace ServiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : Controller
    {
        private readonly AppDbContext _db;
        public RequestsController(AppDbContext db) => _db = db;

        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateRequest([FromForm] ServiceRequestDto dto, [FromForm] List<IFormFile>? images)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var imagePaths = new List<string>();

            if (images != null && images.Any())
            {
                var uploadDir = Path.Combine("wwwroot", "uploads");
                Directory.CreateDirectory(uploadDir);

                foreach (var image in images)
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                    var savePath = Path.Combine(uploadDir, fileName);
                    using var stream = new FileStream(savePath, FileMode.Create);
                    await image.CopyToAsync(stream);
                    imagePaths.Add($"/uploads/{fileName}");
                }
            }

            var request = new ServiceRequest
            {
                Id = Guid.NewGuid(),
                RequestedByUserId = dto.RequestedByUserId,
                RequestedForUserId = dto.RequestedForUserId,
                Description = dto.Description,
                Region = dto.Region,
                SpecialtyRequired = dto.SpecialtyRequired,
                RequestedTime = DateTime.UtcNow,
                Status = RequestStatus.Pending,
                ImageUrls = imagePaths
            };

            _db.ServiceRequests.Add(request);
            await _db.SaveChangesAsync();
            return Ok(request);
        }
        [Authorize(Roles = "Worker")]
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> AcceptRequest(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var req = await _db.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.RequestedForUserId = userId;
            req.Status = RequestStatus.Accepted;
            await _db.SaveChangesAsync();

            var client = await _db.Users.FindAsync(req.RequestedByUserId);
            var worker = await _db.Users.FindAsync(userId);

            return Ok(new
            {
                clientPhone = client?.PhoneNumber,
                workerPhone = worker?.PhoneNumber
            });
        }

        [Authorize(Roles = "Worker")]
        [HttpDelete("{id}/reject")]
        public async Task<IActionResult> RejectRequest(Guid id)
        {
            var req = await _db.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            _db.ServiceRequests.Remove(req);
            await _db.SaveChangesAsync();

            return Ok("Request rejected and deleted.");
        }


        [Authorize]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelRequest(Guid id)
        {
            var req = await _db.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = RequestStatus.Canceled;
            await _db.SaveChangesAsync();

            return Ok("Request canceled.");
        }

        [Authorize(Roles = "Worker")]
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteRequest(Guid id)
        {
            var req = await _db.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = RequestStatus.Completed;
            req.CompletedTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok("Request completed.");
        }

        [Authorize(Roles = "Worker")]
        [HttpGet("worker/{workerId}")]
        public async Task<IActionResult> GetWorkerRequests(Guid workerId)
        {
            if (workerId == Guid.Empty) return BadRequest("Invalid User ID");

            var activeStatuses = new[] { RequestStatus.Pending, RequestStatus.Accepted };

            var requests = await _db.ServiceRequests
                .Where(r => r.RequestedForUserId == workerId)
                .OrderByDescending(r => r.RequestedTime)
                .ToListAsync();

            var result = new
            {
                Active = requests.Where(r => activeStatuses.Contains(r.Status)),
                Completed = requests.Where(r => r.Status == RequestStatus.Completed)
            };

            return Ok(result);
        }
    }
}
