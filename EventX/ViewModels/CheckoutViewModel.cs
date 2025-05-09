using EventX.Models;

namespace EventX.ViewModels
{
    public class CheckoutViewModel
    {
        public int EventID { get; set; } // để dùng khi POST

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PaymentMethod { get; set; }

        public List<TicketItem> Tickets { get; set; } = new();
        public decimal TotalPrice => Tickets.Sum(t => t.Quantity * t.Price);
    }

}
