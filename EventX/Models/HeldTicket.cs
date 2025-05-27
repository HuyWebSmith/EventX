namespace EventX.Models
{
    public class HeldTicket
    {
        public int HeldTicketID { get; set; }
        public int TicketID { get; set; }
        public Ticket Ticket { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int Quantity { get; set; }

        public DateTime HeldAt { get; set; } = DateTime.UtcNow;
    }
}
