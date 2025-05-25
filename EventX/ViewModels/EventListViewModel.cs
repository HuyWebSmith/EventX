using EventX.Models;

namespace EventX.ViewModels
{
    public class EventListViewModel
    {
        public List<Event> Events { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
