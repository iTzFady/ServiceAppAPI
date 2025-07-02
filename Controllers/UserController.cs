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
using System.Text;

namespace ServiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<User> _passwordHasher;
        public UserController(AppDbContext db, IConfiguration config, IPasswordHasher<User> passwordHasher)
        {
            _db = db;
            _config = config;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto registerRequest) {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
            };
            user.Password = _passwordHasher.HashPassword(user, registerRequest.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok( new {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Region,
                user.WorkerSpecialty
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request) {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY");
            var user = await _db.Users
        .           FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

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
        public async Task<IActionResult> GetAvailableWorkers([FromQuery] Specialty specialty) {
            var query = _db.Users.Where(u => u.Role == UserRole.Worker && u.IsAvailable == true && u.WorkerSpecialty == specialty);
            var workers = await query.ToListAsync();
            return Ok(workers);
        }
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(new {
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

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.Users.ToListAsync());
    }
}
