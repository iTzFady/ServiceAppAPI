using ApiResponseCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceApp.Data;
using ServiceApp.Models;
using ServiceApp.Models.DTOs;
using ServiceApp.Models.Enums;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static ApiResponseCode.ResponseCodes;
namespace ServiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IEmailService _emailService;

        public UserController(AppDbContext db, IConfiguration config, IPasswordHasher<User> passwordHasher, IEmailService emailService)
        {
            _db = db;
            _config = config;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromForm] RegisterDto registerRequest , [FromForm] List<IFormFile>? profilePic )
        {

            if (registerRequest == null)
            {
                return BadRequest("Register request is required");
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _db.Users.AnyAsync(u => u.Email == registerRequest.Email))
                return BadRequest(new { message = "Email already registered", code = EMAIL_ALREADY_EXISTS });

            if (!Regex.IsMatch(registerRequest.PhoneNumber, @"^(010|011|012|015)[0-9]{8}$"))
                return BadRequest(new { message = "Invalid phone number format", code = INVALID_PHONE_FORMAT });

            if (await _db.Users.AnyAsync(u => u.PhoneNumber == registerRequest.PhoneNumber))
                return BadRequest(new { message = "Phone number already in use", code = PHONE_ALREADY_EXISTS });

            if (registerRequest.NationalNumber != null && registerRequest.Role == UserRole.Worker)
            {
                if (!Regex.IsMatch(registerRequest.NationalNumber, @"^[23][0-9]{13}$"))
                    return BadRequest(new { message = "Invalid national number format", code = INVALID_NATIONAL_NUMBER });
                if (await _db.Users.AnyAsync(u => u.NationalNumber == registerRequest.NationalNumber))
                    return BadRequest(new { message = "National number already in use", code = NATIONAL_NUMBER_EXISTS });
            }
            string? imagePaths = null;

            if (profilePic != null && profilePic.Any())
            {
                var uploadDir = Path.Combine("wwwroot", "profilePictures");
                Directory.CreateDirectory(uploadDir);
                var image = profilePic.First();
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                var savePath = Path.Combine(uploadDir, fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await image.CopyToAsync(stream);
                imagePaths = $"/profilePictures/{fileName}";
            }
            var user = new User
            {
                Name = registerRequest.Name,
                Role = registerRequest.Role,
                Email = registerRequest.Email,
                PhoneNumber = registerRequest.PhoneNumber,
                NationalNumber = registerRequest.NationalNumber,
                Region = registerRequest.Region,
                WorkerSpecialty = registerRequest.WorkerSpecialty,
                profilePictureUrl= imagePaths,
                // EmailConfirmationToken = GenerateToken(),
                // EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24)
            };
            user.Password = _passwordHasher.HashPassword(user, registerRequest.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Registration successful", code = ResponseCodes.REGISTRATION_SUCCESSFUL });
            // var confirmationLink = $"{Environment.GetEnvironmentVariable("CLIENT_URL")}/api/user/confirm-email?token={user.EmailConfirmationToken}&email={user.Email}";
            // await _emailService.SendEmailAsync(
            //     user.Email,
            //     "Confirm your email",
            //     $"Please confirm your email by clicking <a href='{confirmationLink}'>here</a>");
            // return Ok("Registration successful. Please check your email to confirm your account.");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY");
            var user = await _db.Users
        .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials", code = INVALID_CREDENTIALS });
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid credentials", code = INVALID_CREDENTIALS });
            }
            if (!user.EmailConfirmed)
                return Unauthorized(new { message = "Please confirm your email before logging in", EMAIL_NOT_CONFIRMED });
            if (user.IsBanned)
                return Unauthorized(new { message = "User is Banned", code = USER_BANNED });

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Role,
                    user.IsAvailable,
                },
                code = LOGIN_SUCCESSFUL
            });
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return BadRequest("User not found");
            if (user.EmailConfirmed)
                return BadRequest("Email already confirmed");
            if (user.EmailConfirmationToken != dto.Token ||
                user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired token");
            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiry = null;
            await _db.SaveChangesAsync();
            return Ok("Email confirmed successfully");
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Ok();
            user.PasswordResetToken = GenerateToken();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _db.SaveChangesAsync();
            var resetLink = $"{Environment.GetEnvironmentVariable("CLIENT_URL")}/api/user/reset-password?token={user.PasswordResetToken}&email={user.Email}";
            await _emailService.SendEmailAsync(
                user.Email,
                "Reset your password",
                $"Please reset your password by clicking <a href='{resetLink}'>here</a>"
            );
            return Ok("If an account with this email exists, a password reset link has been sent");
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string token, [FromBody] ResetPasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
            if (user == null)
                return BadRequest("Invalid or expired token");
            if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                return BadRequest("Token has expired");
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("Password don't match");
            user.Password = _passwordHasher.HashPassword(user, dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _db.SaveChangesAsync();
            return Ok("Password has been changed successfully");
        }
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [Authorize]
        [HttpGet("workers")]
        public async Task<IActionResult> GetAvailableWorkers([FromQuery] bool allWorkers = false)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID claim is missing");

            var currentUserId = Guid.Parse(userIdClaim);

            var query = _db.Users.Where(allWorkers ? u => u.Role == UserRole.Worker && u.Id != currentUserId : u => u.Role == UserRole.Worker && u.IsAvailable == true && u.Id != currentUserId)
                                .Select(u => new
                                {
                                    u.Id,
                                    u.Name,
                                    u.WorkerSpecialty,
                                    u.IsAvailable,
                                    u.Region,
                                    u.profilePictureUrl,
                                    AverageRating = _db.Ratings
                                        .Where(r => r.RatedUserId == u.Id)
                                        .Average(r => (double?)r.Stars) ?? 0.0,
                                });
            var workers = await query.ToListAsync();
            return Ok(workers);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!Guid.TryParse(userId, out var id))
                return BadRequest("Invalid user ID");

            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            return Ok(new
            {
                id = userId,
                name = username,
                role = role,
                isAvailable = user.IsAvailable,
                profilePictureUrl = user.profilePictureUrl,
                rating = _db.Ratings
                            .Where(r => r.RatedUserId.ToString() == userId)
                            .Average(r => (double?)r.Stars) ?? 0.0,
            });
        }
        [HttpGet("profile/{id}")]
        public async Task <IActionResult> GetUserById([FromRoute] Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");
            return Ok(new
            {
                name = user.Name,
                role = user.Role,
                isAvailable = user.IsAvailable,
                specialty = user.WorkerSpecialty,
                profilePictureUrl = user.profilePictureUrl,
                rating = _db.Ratings
                                .Where(r => r.RatedUserId.ToString() == id.ToString())
                                .Average(r => (double?)r.Stars) ?? 0.0,
            });
        }


        [Authorize(Roles = "Worker")]
        [HttpPut("availability")]
        public async Task<IActionResult> UpdateAvailability([FromBody] bool isAvailable)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var worker = await _db.Users.FindAsync(userId);

            if (worker == null)
                return NotFound("Worker not found.");

            worker.IsAvailable = isAvailable;
            await _db.SaveChangesAsync();

            return Ok(new { worker.IsAvailable });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.Users.ToListAsync());

        private string GenerateToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("/", "_")
            .Replace("+", "-");
        }


        [HttpPost("pushToken")]
        public async Task<IActionResult> SavePushToken([FromBody] NotificationTokenDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            var otherUsersWithToken = await _db.Users
                    .Where(u => u.ExpoPushToken == dto.Token && u.Id != userId)
                    .ToListAsync();

            foreach (var otherUser in otherUsersWithToken)
            {
                otherUser.ExpoPushToken = null;
            }

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.ExpoPushToken = dto.Token;
            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("deleteToken")]
        public async Task<IActionResult> UnregisterToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID");

            var user = await _db.Users.FindAsync(userId);

            if (user == null) return NotFound();

            var tokenToRemove = user.ExpoPushToken;
            user.ExpoPushToken = null;


            if (!string.IsNullOrEmpty(tokenToRemove))
            {
                var otherUsers = await _db.Users
                    .Where(u => u.ExpoPushToken == tokenToRemove && u.Id != user.Id)
                    .ToListAsync();

                foreach (var otherUser in otherUsers)
                {
                    otherUser.ExpoPushToken = null;
                }
            }
            await _db.SaveChangesAsync();

            return Ok();
        }

    }
}
