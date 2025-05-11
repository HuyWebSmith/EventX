using EventX.Models;

namespace EventX.ViewModels
{
    public class OrderListViewModel
    {
        public List<Order> Orders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public int EventID { get; set; } // Giữ lại event đang lọc
    }
}
