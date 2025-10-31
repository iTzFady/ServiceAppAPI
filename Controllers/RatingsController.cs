using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceApp.Data;
using ServiceApp.Models;
using ServiceApp.Models.DTOs;
using ServiceApp.Models.Enums;
using System.Security.Claims;
using static ApiResponseCode.ResponseCodes;

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
        public async Task<IActionResult> CreateRating([FromBody] RatingDto ratingDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ratedUser = await _db.Users.FindAsync(ratingDto.RatedUserId);
            if (ratedUser == null)
                return NotFound(new { message = "user not found." });

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (userId == ratedUser.Id)
                return BadRequest("You cannot rate yourself.");


            bool ratedExists = await _db.Users
                .AnyAsync(r => r.Id == ratingDto.RatedUserId);


            var serviceRequest = await _db.ServiceRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.RequestedForUser)
                .FirstOrDefaultAsync(r => r.Id == ratingDto.ServiceRequestId);
            if (serviceRequest == null)
                return NotFound(new { message = "Service request not found" , code = REQUEST_NOT_FOUND });

            if (serviceRequest.Status != RequestStatus.Completed && serviceRequest.Status != RequestStatus.Canceled)
                return BadRequest(new {message = "You can only rate completed service requests.", code = REQUEST_STILL_IN_PROGRESS });
            if (serviceRequest.RequestedByUserId != userId && serviceRequest.RequestedForUserId != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not part of this service request" });

            bool ratingExists = await _db.Ratings
                .AnyAsync(r => r.ServiceRequestId == ratingDto.ServiceRequestId && r.RatedByUserId == userId);
            if (ratingExists)
                return BadRequest(new { message = "A rating for this service request already exists." , code = RATING_ALREADY_EXISTS});

            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                ServiceRequestId = ratingDto.ServiceRequestId,
                RatedByUserId = userId,
                RatedUserId = ratingDto.RatedUserId,
                Stars = ratingDto.Stars,
                Comment = ratingDto.Comment,
                CreatedAt = DateTime.Now
            };
            _db.Ratings.Add(rating);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Rating submitted successfully", code = RATING_SUBMITTED });

        }
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetRatingsForWorker(Guid Id)
        {
            var ratings = await _db.Ratings
                .Where(r => r.RatedUserId == Id)
                .ToListAsync();

            var averageStars = ratings.Any() ? ratings.Average(r => r.Stars) : 0;

            return Ok(new
            {
                WorkerId = Id,
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
