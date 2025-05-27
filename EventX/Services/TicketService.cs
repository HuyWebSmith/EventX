using EventX.Enums;
using EventX.Models;

public class TicketService
{
    private readonly ApplicationDbContext _context;

    public TicketService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void UpdateTicketStatus()
    {
        var currentDate = DateTime.Now;

        var soldOutTickets = _context.Tickets
                                      .Where(t => t.Quantity == t.Sold && t.TrangThai != TicketStatus.HetVe)
                                      .ToList();

        foreach (var ticket in soldOutTickets)
        {
            ticket.TrangThai = TicketStatus.HetVe;
        }

        var expiredTickets = _context.Tickets
                                     .Where(t => t.TicketSaleEnd < currentDate && t.TrangThai != TicketStatus.HetHan)
                                     .ToList();

        foreach (var ticket in expiredTickets)
        {

            ticket.TrangThai = TicketStatus.HetHan;
        }

        if (soldOutTickets.Any() || expiredTickets.Any())
        {
            _context.SaveChanges();
        }
    }

    public void ReleaseExpiredHeldTickets()
    {
        var expiredTime = DateTime.UtcNow.AddMinutes(-15);
        var expiredHeldTickets = _context.HeldTickets.Where(h => h.HeldAt < expiredTime).ToList();

        foreach (var expiredHeld in expiredHeldTickets)
        {
            var ticket = _context.Tickets.Find(expiredHeld.TicketID);
            if (ticket != null)
            {
                ticket.Sold -= expiredHeld.Quantity;
                if (ticket.Sold < 0) ticket.Sold = 0;
            }
        }

        _context.HeldTickets.RemoveRange(expiredHeldTickets);
        _context.SaveChanges();
    }
}
