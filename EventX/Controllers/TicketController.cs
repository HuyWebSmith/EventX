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
                TempData["error"] = "Vui lòng cung cấp đầy đủ thông tin";
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
                OrderDetails = new List<OrderDetail>(),
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            // Kiểm tra nếu chọn VNPAY thì chuyển hướng đến trang thanh toán
          

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
            if (model.PaymentMethod == "VNPAY")
            {
                var paymentInfo = new PaymentInformationModel
                {
                    OrderType = "event",
                    OrderDescription = $"Thanh toán đơn hàng #{order.OrderID} ",
                    Amount = totalAmount,
                    Name = user.FullName,
                };

                // Tạo URL thanh toán VNPay
                var vnpayUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);

                return Redirect(vnpayUrl); // Chuyển hướng tới VNPay để thanh toán
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
                                var code = $"TICKET-{detail.Ticket.Type}-{order.OrderID}-{i + 1}-{Guid.NewGuid().ToString("N")[..6]}";

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
