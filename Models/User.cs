using Microsoft.EntityFrameworkCore;
using ServiceApp.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        required public UserRole Role { get; set; }
        required public string Email { get; set; }
        public bool EmailConfirmed { get; set; } = true;
        public string? EmailConfirmationToken { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? EmailConfirmationTokenExpiry { get; set; }

        [Required]
        public string Password { get; set; }
        public string? PasswordResetToken { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? PasswordResetTokenExpiry { get; set; }
        required public string PhoneNumber { get; set; }
        public string? NationalNumber { get; set; }
        public bool? IsAvailable { get; set; } = true;
        public string? Region { get; set; }
        public Specialty? WorkerSpecialty { get; set; }
        public bool IsBanned { get; set; } = false;

        public string? profilePictureUrl { get; set; }
        public string? ExpoPushToken { get; set; }

        public ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>();
        public ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();

        [NotMapped]
        public double AverageRating => RatingsReceived.Any()
    ? RatingsReceived.Average(r => r.Stars)
    : 0;
    }
}
