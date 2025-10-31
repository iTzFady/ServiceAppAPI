using ServiceApp.Models.Enums;

namespace ServiceApp.Models.DTOs
{
    public class ServiceRequestDetailedDto
    {
        public Guid Id { get; set; }
        public string title { get; set; }
        public string Description { get; set; }
        public string location { get; set; }
        public DateTime dateTime { get; set; }
        public string notes { get; set; }
        public DateTime RequestedTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public RequestStatus Status { get; set; }
        public List<string>? ImageUrls { get; set; }

        public UserDto RequestedBy { get; set; }
        public UserDto RequestedFor { get; set; }
        public bool HasRated { get; set; }
        public bool HasReported { get; set; }
    }
}
