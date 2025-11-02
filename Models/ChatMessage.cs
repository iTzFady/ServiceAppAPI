using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceApp.Models
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        [Column(TypeName = "timestamp without time zone")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool isImage { get; set; } = false;
    }
}
