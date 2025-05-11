using EventX.Models;

namespace EventX.ViewModels
{
    public class TicketDetailListViewModel
    {
        public List<OrderDetail> TicketDetails { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int EventID { get; set; }
    }
}
