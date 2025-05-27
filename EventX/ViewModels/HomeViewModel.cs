using EventX.Models;

namespace EventX.ViewModels
{
    public class HomeViewModel
    {
        public List<Event> Events { get; set; }
        public List<Slider> Sliders { get; set; }
        public List<Event>? EventsThisWeekend { get; set; }  
        public List<Event>? EventsThisMonth { get; set; }
    }
}
