using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventX.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }

        [Required]
        [ForeignKey("Order")]
        public int OrderID { get; set; }

        [Required]
        [ForeignKey("Ticket")]
        public int TicketID { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; } // Giá tại thời điểm đặt

        // Quan hệ với Order
        public required Order Order { get; set; }

        // Quan hệ với Ticket
        public required Ticket Ticket { get; set; }
    }
}
