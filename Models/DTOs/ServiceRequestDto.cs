using ServiceApp.Models.Enums;

namespace ServiceApp.Models.DTOs
{
    public class ServiceRequestDto
    {
        public Guid RequestedByUserId { get; set; }
        public Guid RequestedForUserId { get; set; }
        public string Description { get; set; }
        public string Region { get; set; }

        public Specialty SpecialtyRequired { get; set; }
    }
}

