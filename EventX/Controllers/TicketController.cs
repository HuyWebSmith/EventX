using EventX.Extensions;
using EventX.Models;
using EventX.Repositories;
using EventX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Controllers
{
    [Authorize]
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
            var cart = HttpContext.Session.GetObjectFromJson<TicketCart>("TicketCart") ?? new TicketCart();

            // Truyền sự kiện và giỏ vé hiện tại vào view
            var model = new TicketSelectionViewModel
            {
                Event = evt,
                Cart = cart
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddTicket([FromBody] List<TicketItem> cartItems)
        {
            if (cartItems == null || cartItems.Count == 0)
                return BadRequest("Không có vé nào được gửi lên.");

            var cart = HttpContext.Session.GetObjectFromJson<TicketCart>("TicketCart") ?? new TicketCart();

            foreach (var item in cartItems)
            {
                var existing = cart.Items.FirstOrDefault(i => i.TicketId == item.TicketId);
                if (existing != null)
                {
                    existing.Quantity += item.Quantity;
                }
                else
                {
                    cart.Items.Add(item);
                }
            }

            HttpContext.Session.SetObjectAsJson("TicketCart", cart);
            return Ok();
        }



        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<TicketCart>("TicketCart");
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Home");

            // Lưu thời gian giữ vé
            HttpContext.Session.SetString("HoldStartTime", DateTime.UtcNow.ToString());

            var model = new CheckoutViewModel
            {
                Tickets = cart.Items
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            var cart = HttpContext.Session.GetObjectFromJson<TicketCart>("TicketCart");

            // Kiểm tra thời gian giữ vé
            var holdStart = HttpContext.Session.GetString("HoldStartTime");
            if (DateTime.TryParse(holdStart, out var startTime))
            {
                if (DateTime.UtcNow - startTime > TimeSpan.FromMinutes(15))
                {
                    HttpContext.Session.Remove("TicketCart");
                    return RedirectToAction("Timeout");
                }
            }

            if (!ModelState.IsValid) return View(model);

            // Lưu thông tin booking vào DB (thực hiện lưu thông tin vào cơ sở dữ liệu)

            HttpContext.Session.Remove("TicketCart");
            HttpContext.Session.Remove("HoldStartTime");

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Timeout()
        {
            return View();
        }


    }
}
