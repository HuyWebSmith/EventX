using EventX.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TicketReservation
{
    [Key]
    public int ReservationID { get; set; }

    public int TicketID { get; set; }
    public int Quantity { get; set; }

    public DateTime ReservedAt { get; set; }
    public DateTime ExpiryAt { get; set; }

    public bool IsReserved { get; set; } = false; // Vé đã được xác nhận thanh toán

    [ForeignKey("TicketID")]
    public Ticket Ticket { get; set; }
}
