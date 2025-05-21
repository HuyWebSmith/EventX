using EventX.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventX.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public NotificationType Type { get; set; } // VD: Approved, Rejected...

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
