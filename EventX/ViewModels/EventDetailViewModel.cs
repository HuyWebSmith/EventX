using EventX.Models;

namespace EventX.ViewModels
{
    public class EventDetailViewModel
    {
        public Event Event { get; set; }
        public List<Ticket> Tickets { get; set; }
        public List<Review> Reviews { get; set; } // Thêm dòng này
    }
}
