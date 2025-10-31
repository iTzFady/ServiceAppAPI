using ServiceApp.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceApp.Models.DTOs
{
    public class ServiceRequestDto
    {
        public Guid RequestedByUserId { get; set; }
        public Guid RequestedForUserId { get; set; }
        public string title { get; set; }
        public string Description { get; set; }
        public string location { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime dateTime { get; set; }
        public string notes { get; set; }

    }
}

