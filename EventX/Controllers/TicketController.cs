using EventX.Enums;
using EventX.Extensions;
using EventX.Models;
using EventX.Models.VNPAY;
using EventX.Repositories;
using EventX.Services;
using EventX.Services.VNPay;
using EventX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EventX.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly IEventRepository _eventRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _configuration;
        public TicketController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEventRepository eventRepository,
            IConfiguration configuration,
            IVnPayService vnPayService,
            IConfiguration config)           
        {
            _context = context;
            _userManager = userManager;
            _eventRepository = eventRepository;
            _vnPayService = vnPayService;
            _configuration = config;
        }

        [HttpPost]
        public async Task<IActionResult> CheckHeldTickets([FromBody] List<TicketSelectionDto> selectedTickets)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Giả sử bạn có service hoặc DbContext
            foreach (var ticket in selectedTickets)
            {
                var heldTicket = await _context.HeldTickets
                    .Where(ht => ht.UserId == userId && ht.HeldTicketID == ticket.TicketId)
                    .FirstOrDefaultAsync();

                if (heldTicket != null)
                {
                    // Trả về thông tin cần thiết
                    return Json(new { hasHeldTicket = true, ticketId = ticket.TicketId, message = "Bạn đang giữ vé chưa thanh toán. Bạn có muốn tiếp tục không?" });
                }
            }

            return Json(new { hasHeldTicket = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHeldTicket([FromBody] int ticketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var heldTicket = await _context.HeldTickets
                .Where(ht => ht.UserId == userId && ht.HeldTicketID == ticketId)
                .FirstOrDefaultAsync();

            if (heldTicket != null)
            {
                _context.HeldTickets.Remove(heldTicket);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Không tìm thấy vé giữ." });
        }


        [Route("Ticket/Select/{eventId}")]
        public async Task<IActionResult> Select(int eventId)
        {
            var evt = await _eventRepository.GetEventWithTicketsAsync(eventId);
            if (evt == null) return NotFound();

            // Lấy giỏ vé từ session, nếu chưa có thì tạo mới
            var cart = HttpContext.Session.GetObjectFromJson<TicketCart>("TicketCart") ?? new TicketCart();

            bool hasOldCart = false;
            int remainMinutes = 0;

            if (cart.Items.Any())
            {
                var holdStartString = HttpContext.Session.GetString("HoldStartTime");
                if (!string.IsNullOrEmpty(holdStartString) && DateTime.TryParse(holdStartString, out var holdStart))
                {
                    var elapsed = DateTime.UtcNow - holdStart;
                    if (elapsed < TimeSpan.FromMinutes(15))
                    {
                        hasOldCart = true;
                        remainMinutes = 15 - (int)elapsed.TotalMinutes;
                    }
                    else
                    {
                        // Hết hạn thì xóa giỏ cũ
                        HttpContext.Session.Remove("TicketCart");
                        HttpContext.Session.Remove("HoldStartTime");
                        cart = new TicketCart(); // tạo lại giỏ mới rỗng
                    }
                }
            }

            // Truyền dữ liệu ra view
            var model = new TicketSelectionViewModel
            {
                Event = evt,
                Cart = cart
            };

            ViewBag.ShowHoldModal = hasOldCart;
            ViewBag.RemainMinutes = remainMinutes;

            return View(model);
        }


        [HttpPost]
        public IActionResult AddTicket([FromBody] List<TicketItem> cartItems)
        {
            var expiredTime = DateTime.UtcNow.AddMinutes(-15);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Xóa vé giữ quá hạn (của mọi người)
            var expiredHeldTickets = _context.HeldTickets.Where(h => h.HeldAt < expiredTime).ToList();
            foreach (var expiredHeld in expiredHeldTickets)
            {
                var ticket = _context.Tickets.FirstOrDefault(t => t.TicketID == expiredHeld.TicketID);
                if (ticket != null)
                {
                    ticket.Sold -= expiredHeld.Quantity;
                    if (ticket.Sold < 0) ticket.Sold = 0;
                }
            }
            _context.HeldTickets.RemoveRange(expiredHeldTickets);
            _context.SaveChanges();

            // Kiểm tra nếu user đã giữ vé hợp lệ rồi
            var validHeld = _context.HeldTickets
                .Where(h => h.UserId == userId && h.HeldAt >= expiredTime)
                .ToList();

            if (validHeld.Any())
            {
                // Lấy lại giá từng vé từ DB
                var cart = new TicketCart
                {
                    Items = validHeld.Select(h =>
                    {
                        var ticket = _context.Tickets.FirstOrDefault(t => t.TicketID == h.TicketID);
                        return new TicketItem
                        {
                            TicketId = h.TicketID,
                            Quantity = h.Quantity,
                            Price = ticket?.Price ?? 0
                        };
                    }).ToList()
                };

                HttpContext.Session.SetObjectAsJson("TicketCart", cart);
                HttpContext.Session.SetString("HoldStartTime", validHeld.Min(h => h.HeldAt).ToString("o"));
                return Ok(new { message = "Vé đã được giữ trước đó vẫn còn hiệu lực." });
            }

            // Nếu không có vé giữ hợp lệ thì tiến hành giữ mới
            if (cartItems == null || !cartItems.Any())
                return BadRequest("Không có vé nào được gửi lên.");

            // Xóa vé giữ cũ của user (dù còn thời gian hay không)
            var oldHeldTickets = _context.HeldTickets.Where(h => h.UserId == userId).ToList();
            foreach (var oldHeld in oldHeldTickets)
            {
                var ticket = _context.Tickets.FirstOrDefault(t => t.TicketID == oldHeld.TicketID);
                if (ticket != null)
                {
                    ticket.Sold -= oldHeld.Quantity;
                    if (ticket.Sold < 0) ticket.Sold = 0;
                }
            }
            _context.HeldTickets.RemoveRange(oldHeldTickets);
            _context.SaveChanges();

            // Giữ vé mới
            foreach (var item in cartItems)
            {
                var ticket = _context.Tickets.FirstOrDefault(t => t.TicketID == item.TicketId);
                if (ticket == null)
                    return BadRequest($"Vé không tồn tại: {item.TicketId}");

                var available = ticket.Quantity - ticket.Sold;
                if (available < item.Quantity)
                    return BadRequest($"Không đủ vé cho vé ID {item.TicketId}");

                ticket.Sold += item.Quantity;

                var heldTicket = new HeldTicket
                {
                    TicketID = item.TicketId,
                    Quantity = item.Quantity,
                    UserId = userId,
                    HeldAt = DateTime.UtcNow
                };
                _context.HeldTickets.Add(heldTicket);
            }

            _context.SaveChanges();

            // Lưu session
            HttpContext.Session.SetObjectAsJson("TicketCart", new TicketCart { Items = cartItems });
            HttpContext.Session.SetString("HoldStartTime", DateTime.UtcNow.ToString("o"));

            return Ok();
        }


        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<TicketCart>("TicketCart");
            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Home");

            // Lưu thời gian bắt đầu giữ vé nếu chưa lưu
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("HoldStartTime")))
            {
                HttpContext.Session.SetString("HoldStartTime", DateTime.UtcNow.ToString("o")); // dùng chuẩn ISO 8601
            }

            // Lấy vé đầu tiên để lấy EventID
            var ticketItem = cart.Items.FirstOrDefault();
            if (ticketItem == null)
                return RedirectToAction("Index", "Home");

            var ticket = _context.Tickets.FirstOrDefault(t => t.TicketID == ticketItem.TicketId);
            if (ticket == null)
                return RedirectToAction("Index", "Home");

            var eventId = ticket.EventID;

            var model = new CheckoutViewModel
            {
                Tickets = cart.Items,
                EventID = eventId,
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
            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Home");

            // Kiểm tra thời gian giữ vé
            var holdStart = HttpContext.Session.GetString("HoldStartTime");
            if (!string.IsNullOrEmpty(holdStart) && DateTime.TryParse(holdStart, out var startTime))
            {
                if (DateTime.UtcNow - startTime > TimeSpan.FromMinutes(15))
                {
                    // Hết thời gian giữ vé
                    HttpContext.Session.Remove("TicketCart");
                    HttpContext.Session.Remove("HoldStartTime");
                    TempData["Error"] = "Thời gian giữ vé đã hết hạn. Vui lòng chọn lại vé.";
                    return RedirectToAction("Timeout"); // hoặc trang thông báo timeout
                }
            }
            else
            {
                // Nếu không có thời gian giữ vé, chuyển về trang đặt vé lại
                TempData["Error"] = "Không tìm thấy thời gian giữ vé. Vui lòng chọn lại vé.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng cung cấp đầy đủ thông tin.";
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = _userManager.FindByIdAsync(userId).Result;

            var totalAmount = cart.Items.Sum(i => i.Quantity * i.Price);

            var order = new Order
            {
                UserID = userId,
                ApplicationUser = user,
                TotalAmount = totalAmount,
                OrderStatus = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>(),
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            // Lấy vé thật từ DB
            var ticketIds = cart.Items.Select(i => i.TicketId).ToList();
            var realTickets = _context.Tickets
                .Where(t => ticketIds.Contains(t.TicketID))
                .ToDictionary(t => t.TicketID);

            var orderDetails = new List<OrderDetail>();
            foreach (var item in cart.Items)
            {
                if (!realTickets.TryGetValue(item.TicketId, out var realTicket))
                    continue;

                orderDetails.Add(new OrderDetail
                {
                    TicketID = item.TicketId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Order = order,
                    Ticket = realTicket
                });
            }

            order.OrderDetails = orderDetails;

            _context.Order.Add(order);

            // Xóa vé giữ của user
            var heldTickets = _context.HeldTickets.Where(h => h.UserId == userId);
            _context.HeldTickets.RemoveRange(heldTickets);

            _context.SaveChanges();

            // Xóa session giữ vé
            HttpContext.Session.Remove("TicketCart");
            HttpContext.Session.Remove("HoldStartTime");

            if (model.PaymentMethod == "VNPAY")
            {
                var paymentInfo = new PaymentInformationModel
                {
                    OrderType = "event",
                    OrderDescription = $"Thanh toán đơn hàng #{order.OrderID}",
                    Amount = totalAmount,
                    Name = user.FullName,
                };

                var vnpayUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);
                return Redirect(vnpayUrl);
            }

            return RedirectToAction("Success");
        }



        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            var currentUser = await _userManager.GetUserAsync(User);
            if (response.VnPayResponseCode == "00")
            {
                var match = Regex.Match(response.OrderDescription, @"#(\d+)");
                if (match.Success)
                {
                    var orderId = int.Parse(match.Groups[1].Value);

                    var order = _context.Order
                        .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Ticket)
                        .FirstOrDefault(o => o.OrderID == orderId);

                    if (order != null)
                    {
                        order.OrderStatus = OrderStatus.Paid;

                        foreach (var detail in order.OrderDetails)
                        {
                            for (int i = 0; i < detail.Quantity; i++)
                            {
                                string ticketTypeName = detail.Ticket.Type.ToString(); // default là tên enum

                                if (detail.Ticket.Type == TicketType.Custom)
                                {
                                    var fieldInfo = detail.Ticket.Type.GetType().GetField(detail.Ticket.Type.ToString());
                                    if (fieldInfo != null)
                                    {
                                        var display = (DisplayAttribute?)fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
                                        if (display != null && !string.IsNullOrEmpty(display.Name))
                                        {
                                            ticketTypeName = display.Name;
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(detail.Ticket.CustomType))
                                {
                                    ticketTypeName = detail.Ticket.CustomType;
                                }

                                var code = $"TICKET-{ticketTypeName}-{order.OrderID}-{i + 1}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";



                                var issuedTicket = new IssuedTicket
                                {
                                    TicketCode = code,
                                    OrderDetailID = detail.OrderDetailID,
                                    UserId = currentUser.Id,
                                };

                                _context.IssuedTickets.Add(issuedTicket);
                            }

                        }

                        await _context.SaveChangesAsync();

                        return View("PaymentSuccess", response);
                    }

                    return View("PaymentFail", new { Message = "Không tìm thấy thông tin đơn hàng" });
                }

                return View("PaymentFail", new { Message = "Không tìm thấy mã đơn hàng trong mô tả giao dịch" });
            }

            return View("PaymentFail", new { Message = "Thanh toán thất bại" });
        }

        public async Task<IActionResult> MyTickets()
        {
            var userId = _userManager.GetUserId(User);
            var now = DateTime.Now;

            var tickets = await _context.IssuedTickets
                .Include(t => t.OrderDetail)
                    .ThenInclude(od => od.Ticket)
                        .ThenInclude(ti => ti.Event)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.SoldDate)
                .ToListAsync();
            foreach (var ticket in tickets)
            {
                var ticketEntity = ticket.OrderDetail.Ticket;
                var eventEntity = ticketEntity.Event;

                var ticketStart = ticketEntity.StartDate; // ngày bắt đầu riêng của vé

                if (ticketStart > now && ticketStart <= now.AddDays(1)) // vé sắp diễn ra
                {
                    bool alreadyNotified = _context.Notifications.Any(n =>
                        n.UserId == userId &&
                        n.Message.Contains(ticketEntity.Description) && // hoặc tên sự kiện
                        n.CreatedAt.Date == now.Date);

                    if (!alreadyNotified)
                    {
                        var notification = new Notification
                        {
                            UserId = userId,
                            Message = $"⏰ Vé \"{ticketEntity.Description}\" của sự kiện \"{eventEntity.Title}\" sẽ diễn ra lúc {ticketStart:HH:mm dd/MM/yyyy}.",
                            Type = NotificationType.ThongBao,
                            CreatedAt = now,
                            IsRead = false
                        };
                        _context.Notifications.Add(notification);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return View(tickets);
        }

    }
}
