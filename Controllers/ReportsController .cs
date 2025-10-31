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
        [HttpPost("report/{reportedUserId}")]
        public async Task<IActionResult> ReportUser([FromRoute] Guid reportedUserId, [FromBody] ReportRequestDto report)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (userId == reportedUserId)
                return BadRequest("You cannot report yourself.");

            var hasWorkedTogether = await _db.ServiceRequests.AnyAsync(r =>
                r.RequestedByUserId == userId || r.RequestedForUserId == userId &&
                r.RequestedForUserId == reportedUserId || r.RequestedByUserId == reportedUserId &&
                r.Status == RequestStatus.Completed);

            if (!hasWorkedTogether)
                return BadRequest("You can only report a worker you have previously worked with.");

            bool existingReport = await _db.Reports
                 .AnyAsync(r => r.ReportedUserId == reportedUserId && r.ReportedByUserId == userId);

            if (existingReport)
                return BadRequest("You have already reported this worker.");

            var reportRequest = new Report
            {
                Id = Guid.NewGuid(),
                ReportedUserId = reportedUserId,
                ReportedByUserId = userId,
                Reason = report.Report
            };

            _db.Reports.Add(reportRequest);
            await _db.SaveChangesAsync();

            int reportCount = await _db.Reports
                .Where(r => r.ReportedUserId == reportedUserId)
                .Select(r => r.ReportedByUserId)
                .Distinct()
                .CountAsync();

            if (reportCount >= 2)
            {
                var reportedUser = await _db.Users.FindAsync(reportedUserId);
                if (reportedUser != null && !reportedUser.IsBanned)
                {
                    reportedUser.IsBanned = true;
                    await _db.SaveChangesAsync();
                }
            }
            return Ok("Report Submitted.");

        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserReports(Guid id)
        {
            var reports = await _db.Reports
                .Include(r => r.ReportedByUser)
                .Include(r => r.ReportedUser)
                .Where(r => r.ReportedUserId == id)
                .Select(r => new
                {
                    r.Id,
                    r.ReportedByUserId,
                    ReportedByUserName = r.ReportedByUser.Name,
                    r.ReportedUserId,
                    ReportedUserName = r.ReportedUser.Name,
                    r.Reason,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }
    }
}
