using EventX.Enums;
using EventX.Extensions;
using EventX.Models;
using EventX.Repositories;
using EventX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventX.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly IEventRepository _eventRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,IEventRepository eventRepository)
        {
            _context = context;
            _userManager = userManager;
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

            // Ghi đè giỏ hàng thay vì cộng dồn
            var newCart = new TicketCart
            {
                Items = cartItems
            };

            HttpContext.Session.SetObjectAsJson("TicketCart", newCart);
            return Ok();
        }



        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<TicketCart>("TicketCart");
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Home");

            // Lưu thời gian giữ vé
            HttpContext.Session.SetString("HoldStartTime", DateTime.UtcNow.ToString());

            // Lấy TicketId từ vé đầu tiên trong giỏ
            var ticketItem = cart.Items.FirstOrDefault();
            if (ticketItem == null) return RedirectToAction("Index", "Home");

            // Truy vấn vé từ cơ sở dữ liệu để lấy EventID
            var ticket = _context.Tickets.FirstOrDefault(t => t.TicketID == ticketItem.TicketId);
            if (ticket == null) return RedirectToAction("Index", "Home");

            // Lấy EventID từ vé và truy vấn sự kiện
            var eventId = ticket.EventID;


            // Tạo CheckoutViewModel
            var model = new CheckoutViewModel
            {
                Tickets = cart.Items,
                EventID = eventId
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult ClearSession()
        {
            HttpContext.Session.Clear();
            return Ok();
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


            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"Lỗi ở trường '{key}': {error.ErrorMessage}");
                    }
                }

                return View(model);
            }


            // Lấy UserID từ Identity
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userManager.FindByIdAsync(userId).Result;

            var totalAmount = cart.Items.Sum(item => item.Quantity * item.Price);
            // Tạo đối tượng Order
            var order = new Order
            {
                UserID = userId,
                ApplicationUser = user,
                TotalAmount = totalAmount,
                OrderStatus = OrderStatus.Pending, // Trạng thái mặc định
                CreatedAt = DateTime.Now, // Thời gian tạo đơn hàng
                OrderDetails = new List<OrderDetail>()
            };

            // Tạo OrderDetails từ các vé trong giỏ hàng
            // 1. Lấy danh sách TicketID từ giỏ hàng
            var ticketIds = cart.Items.Select(i => i.TicketId).ToList();

            // 2. Truy vấn tất cả vé cùng lúc từ DB
            var realTickets = _context.Tickets
                .Where(t => ticketIds.Contains(t.TicketID))
                .ToDictionary(t => t.TicketID);

            // 3. Tạo danh sách OrderDetail
            var orderDetails = new List<OrderDetail>();
            foreach (var item in cart.Items)
            {
                if (!realTickets.TryGetValue(item.TicketId, out var realTicket))
                    continue; // hoặc xử lý lỗi nếu cần

                orderDetails.Add(new OrderDetail
                {
                    TicketID = item.TicketId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Order = order,
                    Ticket = realTicket
                });
            }

            // Lưu order vào cơ sở dữ liệu
            _context.Order.Add(order);
            _context.OrderDetail.AddRange(orderDetails);
            _context.SaveChanges();

            // Xoá giỏ hàng trong session
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
