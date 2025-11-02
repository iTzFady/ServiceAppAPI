using ServiceApp.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceApp.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } 
        public string body { get; set; }
        public string? Data { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime ReceivedAt { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Alert;
    }
}
