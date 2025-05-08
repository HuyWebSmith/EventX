using EventX.Extensions;
using EventX.Models;
using EventX.Repositories;
using EventX.ViewModels;
using Microsoft.AspNetCore.Mvc;

public class TicketController : Controller
{
    private readonly IEventRepository _eventRepository;

    public TicketController(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    [Route("Ticket/Select/{eventId}")]
    public async Task<IActionResult> Select(int eventId)
    {
        var evt = await _eventRepository.GetEventWithTicketsAsync(eventId);
        if (evt == null) return NotFound();

        // Lấy giỏ vé từ session, nếu chưa có thì tạo mới
        var cart = HttpContext.Session.GetObjectFromJson<TicketSelection>("TicketCart") ?? new TicketSelection();

        // Truyền sự kiện và giỏ vé hiện tại vào view
        var model = new TicketSelectionViewModel
        {
            Event = evt,
            Cart = cart
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult AddTicket(int ticketId, int quantity)
    {
        var tickets = HttpContext.Session.GetObjectFromJson<List<Ticket>>("Tickets");
        var ticket = tickets?.FirstOrDefault(t => t.TicketID == ticketId);

        if (ticket == null) return BadRequest();

        var item = new TicketItem
        {
            TicketId = ticket.TicketID,
            Name = ticket.Description,
            Price = ticket.Price,
            Quantity = quantity
        };

        var cart = HttpContext.Session.GetObjectFromJson<TicketSelection>("TicketCart") ?? new TicketSelection();
        cart.AddTicket(item);

        // Lưu giỏ vé vào session
        HttpContext.Session.SetObjectAsJson("TicketCart", cart);

        return RedirectToAction("Select", new { eventId = ticket.EventID });
    }
}
