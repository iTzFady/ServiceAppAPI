using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceApp.Models
{
    public class Rating
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ServiceRequestId { get; set; }

        [Required]
        public Guid RatedByUserId { get; set; }

        [Required]
        public Guid RatedUserId { get; set; }
        [Range(1, 5)]
        public int Stars { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ServiceRequestId")]
        public ServiceRequest serviceRequest { get; set; } = null;

        [ForeignKey("RatedByUserId")]
        public User RatedBy { get; set; } = null;

        [ForeignKey("RatedUserId")]
        public User RatedUser { get; set; } = null;
    }
}
