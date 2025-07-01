using ServiceApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ServiceApp.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        required public string Name { get; set; }
        public UserRole Role { get; set; }
        required public string Email { get; set; }
        required public string Password { get; set; }
        required public string PhoneNumber { get; set; }
        public string? NationalNumber { get; set; }
        public bool? IsAvailable { get; set; }
        required public string Region { get; set; }
        public Specialty? WorkerSpecialty { get; set; }
        public bool IsBanned { get; set; } = false;
    }
}
