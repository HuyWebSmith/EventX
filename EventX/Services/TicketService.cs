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

        // Tìm các vé đã hết vé
        var soldOutTickets = _context.Tickets
                                      .Where(t => t.Quantity == t.Sold && t.TrangThai != TicketStatus.HetVe)
                                      .ToList();

        foreach (var ticket in soldOutTickets)
        {
            // Cập nhật trạng thái vé thành "Hết vé"
            ticket.TrangThai = TicketStatus.HetVe;
        }

        // Tìm các vé đã hết hạn
        var expiredTickets = _context.Tickets
                                     .Where(t => t.TicketSaleEnd < currentDate && t.TrangThai != TicketStatus.HetHan)
                                     .ToList();

        foreach (var ticket in expiredTickets)
        {
            // Cập nhật trạng thái vé thành "Hết hạn"
            ticket.TrangThai = TicketStatus.HetHan;
        }

        // Lưu lại các thay đổi vào cơ sở dữ liệu
        if (soldOutTickets.Any() || expiredTickets.Any())
        {
            _context.SaveChanges();
        }
    }
}
