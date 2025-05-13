using EventX.Models;

namespace EventX.ViewModels
{
    public class TicketListViewModel
    {
        public List<OrderDetail> Tickets { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int EventID { get; set; }
    }
}
