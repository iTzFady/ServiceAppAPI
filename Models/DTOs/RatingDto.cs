using System.ComponentModel.DataAnnotations;

namespace ServiceApp.Models.DTOs
{
    public class RatingDto
    {
        public Guid ServiceRequestId { get; set; }
        public Guid RatedByUserId { get; set; }
        public Guid RatedUserId { get; set; }
        [Range(1, 5)]
        public int Stars { get; set; }
        public string? Comment { get; set; }
    }
}
