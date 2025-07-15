using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceApp.Data;
using ServiceApp.Models;
using ServiceApp.Models.DTOs;
using ServiceApp.Models.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto registerRequest)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Regex.IsMatch(registerRequest.PhoneNumber, @"^(010|011|012|015)[0-9]{8}$"))
                return BadRequest("Invalid phone number format.");

            if (await _db.Users.AnyAsync(u => u.PhoneNumber == registerRequest.PhoneNumber))
                return BadRequest("Phone number already in use");

            if (registerRequest.NationalNumber != null)
            {
                if (!Regex.IsMatch(registerRequest.NationalNumber, @"^[23][0-9]{13}$"))
                    return BadRequest("Invalid national number format.");
                if (await _db.Users.AnyAsync(u => u.NationalNumber == registerRequest.NationalNumber))
                    return BadRequest("National number already in use");
            }

            if (await _db.Users.AnyAsync(u => u.Email == registerRequest.Email))
                return BadRequest("Email already registered");

            var user = new User
            {
                Name = registerRequest.Name,
                Role = registerRequest.Role,
                Email = registerRequest.Email,
                PhoneNumber = registerRequest.PhoneNumber,
                NationalNumber = registerRequest.NationalNumber,
                Region = registerRequest.Region,
                WorkerSpecialty = registerRequest.WorkerSpecialty,
                EmailConfirmationToken = GenerateToken(),
                EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24)
            };
            user.Password = _passwordHasher.HashPassword(user, registerRequest.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var confirmationLink = $"{Environment.GetEnvironmentVariable("CLIENT_URL")}/api/user/confirm-email?token={user.EmailConfirmationToken}&email={user.Email}";
            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $"Please confirm your email by clicking <a href='{confirmationLink}'>here</a>");
            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Region,
                user.WorkerSpecialty,
                Message = "Registration successful. Please check your email to confirm your account."
            });
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
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }
            if (!user.EmailConfirmed)
                return Unauthorized("Please confirm your email before logging in");
            if (user.IsBanned)
                return Unauthorized("User is Banned");

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.Region,
                }
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
            var resetLink = $"{Environment.GetEnvironmentVariable("CLIENT_URL")}/reset-password?token={user.PasswordResetToken}&email={user.Email}";
            await _emailService.SendEmailAsync(
                user.Email,
                "Reset your password",
                $"Please reset your password by clicking <a href='{resetLink}'>here</a>"
            );
            return Ok("If an account with this email exists, a password reset link has been sent");
        }
        [HttpPost("reset-password/{token}")]
        public async Task<IActionResult> ResetPassword([FromRoute] string token, [FromBody] ResetPasswordDto dto)
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
                new Claim(ClaimTypes.Role, user.Role.ToString())
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
        public async Task<IActionResult> GetAvailableWorkers()
        {
            var query = _db.Users.Where(u => u.Role == UserRole.Worker && u.IsAvailable == true)
                                .Select(u => new
                                {
                                    u.Id,
                                    u.Name,
                                    u.PhoneNumber,
                                    u.WorkerSpecialty,
                                    u.IsAvailable
                                });
            var workers = await query.ToListAsync();
            return Ok(workers);
        }
        //public async Task<IActionResult> GetAvailableWorkers([FromQuery] Specialty specialty) {

        //    var query = _db.Users.Where(u => u.Role == UserRole.Worker && u.IsAvailable == true && u.WorkerSpecialty == specialty);
        //    var workers = await query.ToListAsync();
        //    return Ok(workers);
        //}
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(new
            {
                UserId = userId,
                name = username,
                Role = role
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

            return Ok(new { worker.Id, worker.IsAvailable });
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
    }
}
