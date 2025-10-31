using ServiceApp.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceApp.Models
{
    public class ServiceRequest
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey(nameof(RequestedByUser))]
        public Guid RequestedByUserId { get; set; }
        [ForeignKey(nameof(RequestedForUser))]
        public Guid RequestedForUserId { get; set; }
        public string title { get; set; }
        public string Description { get; set; }
        public string location { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime dateTime { get; set; }
        public string notes { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime RequestedTime { get; set; } = DateTime.Now;
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? CompletedTime { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public List<string>? ImageUrls { get; set; }
        public User RequestedByUser { get; set; }
        public User RequestedForUser { get; set; }


    }
}
