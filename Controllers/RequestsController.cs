using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceApp.Data;
using ServiceApp.Models;
using ServiceApp.Models.DTOs;
using ServiceApp.Models.Enums;

namespace ServiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : Controller
    {
        private readonly AppDbContext _db;
        public RequestsController(AppDbContext db) => _db = db;

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] ServiceRequestDto dto) {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var request = new ServiceRequest
            {
                Id = Guid.NewGuid(),
                RequestedByUserId = dto.RequestedByUserId,
                RequestedForUserId = dto.RequestedForUserId,
                Description = dto.Description,
                Region = dto.Region,
                SpecialtyRequired = dto.SpecialtyRequired,
                RequestedTime = DateTime.UtcNow,
                Status = RequestStatus.Pending
            };

            _db.ServiceRequests.Add(request);
            await _db.SaveChangesAsync();
            return Ok(request);
        }
        [Authorize(Roles = "Worker")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id , [FromQuery] RequestStatus status) {

            if (id == Guid.Empty) return BadRequest("Invalid User ID");

            var req = await _db.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = status;

            if (req.Status == RequestStatus.Completed) req.CompletedTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(req);
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
        [Authorize(Roles = "Worker")]
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> MarkAsCompleted(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invalid User ID");

            var req = await _db.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = RequestStatus.Completed;
            req.CompletedTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(req);
        }
    }
}
