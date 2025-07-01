using ServiceApp.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceApp.Models
{
    public class ServiceRequest
    {
        [Key]
        public Guid Id { get; set; }
        public Guid RequestedByUserId { get; set; }
        public Guid RequestedForUserId { get; set; }
        public string Description { get; set; }
        public string Region { get; set; }
        public Specialty SpecialtyRequired { get; set; }
        public DateTime RequestedTime { get; set; } = DateTime.Now;
        public DateTime? CompletedTime { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        [ForeignKey("RequestedByUserId")]
        public User RequestedBy { get; set; }

        [ForeignKey("RequestedForUserId")]
        public User RequestedFor { get; set; }
    }
}
