using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceApp.Data;
using ServiceApp.Models;
using ServiceApp.Models.Enums;
using System.Security.Claims;

namespace ServiceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ReportsController(AppDbContext db) => _db = db;


        [Authorize]
        [HttpPost("report/{workerId}")]
        public async Task<IActionResult> ReportUser([FromRoute] Guid workerId, [FromBody] ReportRequestDto report)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var existingReport = await _db.Reports.FirstOrDefaultAsync(r => r.ReportedUserId == workerId && r.ReportedByUserId == userId);

            if (existingReport != null)
                return BadRequest("You have already reported this worker.");

            var hasWorkedTogether = await _db.ServiceRequests.AnyAsync(r =>
                 r.RequestedByUserId == userId &&
                 r.RequestedForUserId == workerId &&
                 r.Status == RequestStatus.Completed);

            if (!hasWorkedTogether)
                return BadRequest("You can only report a worker you have previously worked with.");

            var reportRequest = new Report
            {
                Id = Guid.NewGuid(),
                ReportedUserId = workerId,
                ReportedByUserId = userId,
                Reason = report.Report
            };

            _db.Reports.Add(reportRequest);
            await _db.SaveChangesAsync();
            int reportCount = await _db.Reports.CountAsync(r => r.ReportedUserId == workerId);

            if (reportCount >= 2)
            {
                var worker = await _db.Users.FindAsync(workerId);
                if (worker != null && !worker.IsBanned)
                {
                    worker.IsBanned = true;
                    await _db.SaveChangesAsync();
                }
            }
            return Ok(new { message = "Report Submitted." });

        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserReports(Guid id)
        {
            var reports = await _db.Reports
                .Where(r => r.ReportedUserId == id)
                .Select(r => new
                {
                    r.Id,
                    r.ReportedByUserId,
                    r.Reason,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }
    }
}
