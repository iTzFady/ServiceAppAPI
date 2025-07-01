using System.ComponentModel.DataAnnotations;

namespace ServiceApp.Models
{
    public class Report
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ReportedUserId { get; set; }
        [Required]
        public Guid ReportedByUserId { get; set; }

        [Required]
        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
