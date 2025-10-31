using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public User ReportedByUser { get; set; }
        public User ReportedUser { get; set; }
    }
}
