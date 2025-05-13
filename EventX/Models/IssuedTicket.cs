using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventX.Models
{
    public class IssuedTicket
    {
        [Key]
        public int IssuedTicketID { get; set; }

        [Required]
        public string TicketCode { get; set; } = null!;  // Mã vé duy nhất

        [Required]
        [ForeignKey("OrderDetail")]
        public int OrderDetailID { get; set; }

        public OrderDetail OrderDetail { get; set; } = null!;
    }
}
