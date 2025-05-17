using EventX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventX.Areas.Host.Controllers
{
    [Area("Host")]
    [ApiController]
    [Route("Host/api/[controller]")]
    public class CheckinApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CheckinApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetTicketInfo")]
        public async Task<IActionResult> GetTicketInfo(string ticketCode, int eventId)
        {
            if (string.IsNullOrEmpty(ticketCode))
                return BadRequest("Ticket code is required");

            var ticket = await _context.IssuedTickets
                .Include(it => it.OrderDetail)
                    .ThenInclude(od => od.Ticket)
                        .ThenInclude(t => t.Event)
                .FirstOrDefaultAsync(it => it.TicketCode == ticketCode && it.OrderDetail.Ticket.EventID == eventId);

            if (ticket == null)
                return NotFound("Không tìm thấy vé này");

            var eventEntity = ticket.OrderDetail.Ticket.Event;

            return Ok(new
            {
                ticketCode = ticket.TicketCode,
                eventName = eventEntity.Title,
                isCheckedIn = ticket.IsCheckedIn,
                checkinTime = ticket.CheckinTime,
                eventStatus = eventEntity.Status.ToString()
            });
        }

        [HttpPost("Checkin")]
        public async Task<IActionResult> Checkin([FromBody] CheckinRequest request)
        {
            if (string.IsNullOrEmpty(request.TicketCode))
                return BadRequest("Ticket code is required");

            var ticket = await _context.IssuedTickets
                .Include(it => it.OrderDetail)
                    .ThenInclude(od => od.Ticket)
                        .ThenInclude(t => t.Event)
                .FirstOrDefaultAsync(it => it.TicketCode == request.TicketCode &&
        it.OrderDetail.Ticket.EventID == request.EventId);

            if (ticket == null)      
                return NotFound("Không tìm thấy vé này");

            if (ticket.OrderDetail.Ticket.Event.EventID != request.EventId)
                return BadRequest("Vé không thuộc sự kiện này.");

            var eventEntity = ticket.OrderDetail.Ticket.Event;
            // Kiểm tra sự kiện đã kết thúc chưa
            if (eventEntity.Status == Enums.EventStatus.Completed)
            {
                return BadRequest("Sự kiện đã kết thúc, không thể check-in.");
            }
            if (ticket.IsCheckedIn)
                return BadRequest("Vé đã được checkin trước đó");

            // Cập nhật trạng thái check-in
            ticket.IsCheckedIn = true;
            ticket.CheckinTime = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Check-in successful",
                ticketCode = ticket.TicketCode,
                eventName = ticket.OrderDetail.Ticket.Event.Title,
                checkinTime = ticket.CheckinTime
            });
        }
    }


    public class CheckinRequest
    {
        public string TicketCode { get; set; } = null!;
        public int EventId { get; set; }
    }
}
