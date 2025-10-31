using ServiceApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ServiceApp.Models.DTOs
{
    public class RegisterDto
    {
        required public string Name { get; set; }
        public UserRole Role { get; set; }
        [EmailAddress]
        required public string Email { get; set; }
        [StringLength(100, MinimumLength = 6)]
        required public string Password { get; set; }
        required public string PhoneNumber { get; set; }
        public string? NationalNumber { get; set; }
        public string? Region { get; set; }
        public Specialty? WorkerSpecialty { get; set; }
    }
}
