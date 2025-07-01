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
    public class RatingsController : Controller
    {
        private readonly AppDbContext _db;
        public RatingsController(AppDbContext db) => _db = db;
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateRating([FromBody] RatingDto ratingDto) {
            var serviceRequest = await _db.ServiceRequests.FindAsync(ratingDto.ServiceRequestId);
            if (serviceRequest == null)
                return NotFound("Service request not found.");

            if (serviceRequest.Status != RequestStatus.Completed)
                return BadRequest("You can only rate completed service requests.");

            bool ratingExists = await _db.Ratings
                .AnyAsync(r => r.ServiceRequestId == ratingDto.ServiceRequestId);
            if (ratingExists)
                return BadRequest("A rating for this service request already exists.");

            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                ServiceRequestId = ratingDto.ServiceRequestId,
                RatedByUserId = ratingDto.RatedByUserId,
                RatedUserId = ratingDto.RatedUserId,
                Stars = ratingDto.Stars,
                Comment = ratingDto.Comment,
                CreatedAt = DateTime.UtcNow
            };
            _db.Ratings.Add(rating);
            await _db.SaveChangesAsync();
            return Ok(rating);
        }
        [HttpGet("worker/{workerId}")]
        public async Task<IActionResult> GetRatingsForWorker(Guid workerId)
        {
            var ratings = await _db.Ratings
                .Where(r => r.RatedUserId == workerId)
                .ToListAsync();

            var averageStars = ratings.Any() ? ratings.Average(r => r.Stars) : 0;

            return Ok(new
            {
                WorkerId = workerId,
                AverageRating = Math.Round(averageStars, 2),
                TotalRatings = ratings.Count,
                Ratings = ratings.Select(r => new
                {
                    r.Stars,
                    r.Comment,
                    r.RatedByUserId,
                    r.ServiceRequestId,
                    r.CreatedAt
                })
            });
        }


    }
}
