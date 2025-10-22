using Microsoft.EntityFrameworkCore;
using ServiceApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ServiceApp.Models
{
    [Index(nameof(PhoneNumber), IsUnique = true)]
    [Index(nameof(NationalNumber), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        required public string Name { get; set; }
        public UserRole Role { get; set; }
        required public string Email { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpiry { get; set; }

        [Required]
        public string Password { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        required public string PhoneNumber { get; set; }
        public string? NationalNumber { get; set; }
        public bool? IsAvailable { get; set; } = true;
        required public string Region { get; set; }
        public Specialty? WorkerSpecialty { get; set; }
        public bool IsBanned { get; set; } = false;
    }
}
