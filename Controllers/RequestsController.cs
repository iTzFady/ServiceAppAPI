using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServiceApp.Data;
using ServiceApp.Models;
using ServiceApp.Models.DTOs;
using ServiceApp.Models.Enums;
using System.Security.Claims;
using ServiceApp.Hubs;
using static ApiResponseCode.ResponseCodes;

namespace ServiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<RequestsHub> _hubContext;
        public RequestsController(AppDbContext db, IHubContext<RequestsHub> hubContext) {
            _db = db;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateRequest([FromForm] ServiceRequestDto dto, [FromForm] List<IFormFile>? images)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.RequestedByUserId);
            if (!userExists)
                return BadRequest("user not found");

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
                title = dto.title,
                Description = dto.Description,
                location = dto.location,
                dateTime = dto.dateTime,
                notes = dto.notes,
                RequestedTime = DateTime.Now,
                Status = RequestStatus.Pending,
                ImageUrls = imagePaths
            };

            _db.ServiceRequests.Add(request);
            await _db.SaveChangesAsync();

            var fullRequest = await _db.ServiceRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.RequestedForUser)
                .FirstOrDefaultAsync(r => r.Id == request.Id);
            if(fullRequest == null)
                return StatusCode(500, "Error retrieving created request");

            var requestDto = new ServiceRequestDetailedDto
            {
                Id = fullRequest.Id,
                title = fullRequest.title,
                Description = fullRequest.Description,
                location = fullRequest.location,
                dateTime = fullRequest.dateTime,
                notes = fullRequest.notes,
                Status = fullRequest.Status,
                ImageUrls = fullRequest.ImageUrls,
                RequestedBy = new UserDto
                {
                    Id = fullRequest.RequestedByUser.Id,
                    Name = fullRequest.RequestedByUser.Name,
                    rating = (float)fullRequest.RequestedByUser.AverageRating,
                    ProfilePictureUrl = fullRequest.RequestedByUser.profilePictureUrl,
                    PhoneNumber = fullRequest.RequestedByUser.PhoneNumber,
                },
                RequestedFor = new UserDto
                {
                    Id = fullRequest.RequestedForUser.Id,
                    Name = fullRequest.RequestedForUser.Name,
                    rating = (float)fullRequest.RequestedForUser.AverageRating,
                    ProfilePictureUrl = fullRequest.RequestedForUser.profilePictureUrl,
                    PhoneNumber =fullRequest.RequestedForUser.PhoneNumber,
                }
            };

            await _hubContext.Clients.User(dto.RequestedForUserId.ToString()).SendAsync("NewRequestCreated", requestDto);

            return Ok(new {code= REQUEST_CREATED , message= "Request has been created successfully" });
        }
        
        
        [Authorize(Roles = "Worker")]
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> AcceptRequest(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var req = await _db.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            if (req.Status != RequestStatus.Pending && req.Status != RequestStatus.Accepted)
                return BadRequest(new { code = INVALID_REQUEST_STATUS, message = "Request cannot be modified." });


            req.RequestedForUserId = userId;
            req.Status = RequestStatus.Accepted;
            await _db.SaveChangesAsync();

            var client = await _db.Users.FindAsync(req.RequestedByUserId);
            var worker = await _db.Users.FindAsync(userId);

            await _hubContext.Clients.User(req.RequestedByUserId.ToString()).SendAsync("RequestAccepted", new
            {
                requestId = req.Id,
                acceptedBy = userId,
                status = req.Status.ToString()
            });
            await _hubContext.Clients.User(req.RequestedForUserId.ToString()).SendAsync("RequestAccepted", new
            {
                requestId = req.Id,
                acceptedBy = userId,
                status = req.Status.ToString()
            });


            return Ok(new
            {
                message = "Request accepted",
                code = REQUEST_ACCEPTED,
                clientPhone = client?.PhoneNumber,
                workerPhone = worker?.PhoneNumber
            });
        }

        [Authorize(Roles = "Worker")]
        [HttpDelete("{id}/reject")]
        public async Task<IActionResult> RejectRequest(Guid id)
        {
            var req = await _db.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound(new{ code = REQUEST_NOT_FOUND });

            if (req.Status != RequestStatus.Pending && req.Status != RequestStatus.Accepted)
                return BadRequest(new { code = INVALID_REQUEST_STATUS, message = "Request cannot be modified." });

            _db.ServiceRequests.Remove(req);
            await _db.SaveChangesAsync();
            await _hubContext.Clients.User(req.RequestedByUserId.ToString()).SendAsync("RequestRejected", new
            {
                requestId = req.Id,
                status = "Rejected"
            });
            return Ok(new { message = "Request rejected and deleted." ,code = REQUEST_REJECTED });
        }


        [Authorize]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelRequest(Guid id)
        {
            var req = await _db.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = RequestStatus.Canceled;
            await _db.SaveChangesAsync();

            await _hubContext.Clients.User(req.RequestedForUserId.ToString()).SendAsync("RequestCanceled", new
            {
                requestId = req.Id,
                status = req.Status.ToString()
            });

            return Ok(new { message = "Request canceled", code= REQUEST_CANCELLED });
        }

        [Authorize(Roles = "Worker")]
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteRequest(Guid id)
        {
            var req = await _db.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = RequestStatus.Completed;
            req.CompletedTime = DateTime.Now;
            await _db.SaveChangesAsync();
            await _hubContext.Clients.User(req.RequestedByUserId.ToString()).SendAsync("RequestCompleted", new
            {
                requestId = req.Id,
                status = req.Status.ToString(),
                completedTime = req.CompletedTime
            });

            await _hubContext.Clients.User(req.RequestedForUserId.ToString()).SendAsync("RequestCompleted", new
            {
                requestId = req.Id,
                status = req.Status.ToString(),
                completedTime = req.CompletedTime
            });
            return Ok(new{ message = "Request completed" , code = REQUEST_COMPLETED});
        }

        [Authorize(Roles = "Worker")]
        [HttpGet("getWorkerRequests")]
        public async Task<IActionResult> GetWorkerRequests()
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { code = UNAUTHORIZED, message = "User not authenticated." });
            var workerId = Guid.Parse(userIdClaim);
            var activeStatuses = new[] { RequestStatus.Pending, RequestStatus.Accepted };

            var requests = await _db.ServiceRequests
                .Where(r => r.RequestedForUserId == workerId)
                .Where(r=> r.Status == RequestStatus.Pending || r.Status == RequestStatus.Accepted)
                .Include(r => r.RequestedByUser)
                .Include(r => r.RequestedForUser)
                .OrderByDescending(r => activeStatuses.Contains(r.Status))
                .ThenByDescending(r => r.RequestedTime)
                .ToListAsync();

            var reportedUserIds = await _db.Reports
                .Where(r => r.ReportedByUserId == workerId)
                .Select(r => r.ReportedUserId)
                .Distinct()
                .ToListAsync();

            var ratedRequestIds = await _db.Ratings
                .Where(r => r.RatedByUserId == workerId )
                .Select(r => r.ServiceRequestId)
                .Distinct()
                .ToListAsync();

            var result = requests.Select(r => new ServiceRequestDetailedDto
            {
                Id = r.Id,
                title = r.title,
                Description = r.Description,
                location = r.location,
                dateTime = r.dateTime,
                notes = r.notes,
                Status = r.Status,
                ImageUrls = r.ImageUrls,
                CompletedTime = r.CompletedTime,
                RequestedBy = new UserDto
                {
                    Id = r.RequestedByUser.Id,
                    Name = r.RequestedByUser.Name,
                    rating = (float)r.RequestedByUser.AverageRating,
                    ProfilePictureUrl = r.RequestedByUser.profilePictureUrl,
                    PhoneNumber = r.RequestedByUser.PhoneNumber,

                },
                RequestedFor = new UserDto
                {
                    Id = r.RequestedForUser.Id,
                    Name = r.RequestedForUser.Name,
                    rating = (float)r.RequestedForUser.AverageRating,
                    ProfilePictureUrl = r.RequestedForUser.profilePictureUrl,
                    PhoneNumber =  r.RequestedForUser.PhoneNumber,

                },
                HasReported = reportedUserIds.Contains(r.RequestedByUserId),
                HasRated = ratedRequestIds.Contains(r.Id)
            }).ToList();
           
            
            return Ok(result);
        }

        [Authorize]
        [HttpGet("getClientRequests")]
        public async Task<IActionResult> GetRequests([FromHeader]bool isLog = false)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { code = UNAUTHORIZED, message = "User not authenticated." });
            var activeStatuses = new[] { RequestStatus.Pending, RequestStatus.Accepted };
            var Id = Guid.Parse(userIdClaim);

            var requests = await (isLog ? _db.ServiceRequests.Where(r => (r.RequestedByUserId == Id || r.RequestedForUserId == Id )&& !activeStatuses.Contains(r.Status))
                .Include(r => r.RequestedByUser)
                .Include(r => r.RequestedForUser)
                .OrderByDescending(r => r.RequestedTime)
                .ToListAsync()
            : _db.ServiceRequests
                .Where(r => r.RequestedByUserId == Id && activeStatuses.Contains(r.Status))
                .Include(r => r.RequestedByUser)
                .Include(r => r.RequestedForUser)
                .OrderByDescending(r => activeStatuses.Contains(r.Status))
                .OrderByDescending(r => r.RequestedTime)
                .ToListAsync());

            var reportedUserIds = await _db.Reports
                .Where(r => r.ReportedByUserId == Id)
                .Select(r => r.ReportedUserId)
                .Distinct()
                .ToListAsync();

            var ratedRequestIds = await _db.Ratings
                .Where(r => r.RatedByUserId == Id)
                .Select(r => new { r.RatedUserId, r.ServiceRequestId })
                .Distinct()
                .ToListAsync();

            var result = requests.Select(r =>
            {
                var otherUserId = r.RequestedByUserId == Id ? r.RequestedForUserId : r.RequestedByUserId;
                return new ServiceRequestDetailedDto
                {
                    Id = r.Id,
                    title = r.title,
                    Description = r.Description,
                    location = r.location,
                    dateTime = r.dateTime,
                    notes = r.notes,
                    Status = r.Status,
                    ImageUrls = r.ImageUrls,
                    CompletedTime = r.CompletedTime,
                    RequestedBy = new UserDto
                    {
                        Id = r.RequestedByUser.Id,
                        Name = r.RequestedByUser.Name,
                        rating = (float)r.RequestedByUser.AverageRating,
                        ProfilePictureUrl = r.RequestedByUser.profilePictureUrl,
                        PhoneNumber = r.RequestedByUser.PhoneNumber,
                    },
                    RequestedFor = new UserDto
                    {
                        Id = r.RequestedForUser.Id,
                        Name = r.RequestedForUser.Name,
                        rating = (float)r.RequestedForUser.AverageRating,
                        ProfilePictureUrl = r.RequestedForUser.profilePictureUrl,
                        PhoneNumber = r.RequestedForUser.PhoneNumber,
                    },
                    HasReported = reportedUserIds.Contains(otherUserId),
                    HasRated = ratedRequestIds.Any(x => x.ServiceRequestId == r.Id && x.RatedUserId == otherUserId)
                };
            }).ToList();


            return Ok(result);
        }


    }
}
