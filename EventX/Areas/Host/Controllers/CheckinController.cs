using EventX.Models;
using EventX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventX.Areas.Host.Controllers
{
    [Area("Host")]
    public class CheckinController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CheckinController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int eventId)
        {
            var issuedTickets = _context.IssuedTickets
                .Include(t => t.OrderDetail)
                .ThenInclude(od => od.Order)
                .Where(t => t.OrderDetail.Ticket.EventID == eventId)
                .OrderByDescending(t => t.CheckinTime)
                .ToList();

            ViewBag.EventId = eventId;
            return View(issuedTickets);
        }
        public IActionResult QR(int eventId)
        {
            var model = new CheckinViewModel
            {
                EventId = eventId
  
            };
            return View(model);
        }

        // Xử lý check-in thủ công theo mã vé
        [HttpPost]
        public IActionResult ManualCheckIn(string ticketCode)
        {
            var ticket = _context.IssuedTickets
                .Include(t => t.OrderDetail)
                .ThenInclude(od => od.Order)
                .FirstOrDefault(t => t.TicketCode == ticketCode);

            if (ticket == null)
                return Json(new { success = false, message = "Không tìm thấy vé." });

            if (ticket.IsCheckedIn)
                return Json(new { success = false, message = "Vé đã được check-in trước đó." });

            ticket.IsCheckedIn = true;
            ticket.CheckinTime = DateTime.Now;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "✅ Check-in thành công!",
                checkInTime = ticket.CheckinTime?.ToString("dd/MM/yyyy HH:mm:ss")
            });
        }
    }
}
