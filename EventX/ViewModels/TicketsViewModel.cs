using EventX.Extensions;
using EventX.Models;

namespace EventX.ViewModels
{
    public class TicketsViewModel
    {
        public PaginatedList<IssuedTicket> UpcomingTickets { get; set; }
        public PaginatedList<IssuedTicket> PastTickets { get; set; }
    }

}
