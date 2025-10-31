using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceApp.Models
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        [Column(TypeName = "timestamp without time zone")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool isImage { get; set; } = false;
    }
}
