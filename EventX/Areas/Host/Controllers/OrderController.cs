using EventX.Enums;
using EventX.Extensions;
using EventX.Models;
using EventX.Services;
using EventX.Services.Email;
using EventX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventX.Areas.Host.Controllers
{
    [Area("Host")]
    [Authorize(Roles = "Host")]// Chỉ Admin mới có quyền truy cập
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;


        public OrderController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        public IActionResult Index(int eventId, int page = 1, string type = "orders", string searchTerm = "")
        {
            int pageSize = 10;

            // Truy vấn đơn hàng
            var query = _context.Order
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Ticket)
                .Where(o => o.OrderDetails.Any(od => od.Ticket != null && od.Ticket.EventID == eventId))
                .OrderByDescending(o => o.OrderID);

            // Truy vấn vé
            var query1 = _context.OrderDetail
                .Include(od => od.Ticket)  // Bao gồm thông tin Ticket
                .Where(od => od.Ticket.EventID == eventId)  // Lọc theo EventID của Ticket
                .OrderByDescending(od => od.TicketID);

            if (!string.IsNullOrEmpty(searchTerm) && int.TryParse(searchTerm, out int orderId))
            {
                query = query
                    .Where(o => o.OrderID == orderId)
                    .OrderByDescending(o => o.OrderID); // cần gọi lại để giữ IOrderedQueryable

                query1 = query1
                    .Where(od => od.OrderID == orderId)
                    .OrderByDescending(od => od.TicketID); // tương tự nếu cần
            }

            // Tổng số đơn hàng và vé
            var totalOrders = query.Count();
            var totalTickets = query1.Count();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
            var totalPages1 = (int)Math.Ceiling(totalTickets / (double)pageSize);

            // Lấy danh sách đơn hàng
            var orders = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Lấy danh sách vé (thực tế là OrderDetail)
            var tickets = query1
                .Where(od => od.Order != null && od.Order.OrderStatus != null)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                 .Include(od => od.Order)
                .ToList(); 

            // ViewModel cho đơn hàng
            var viewModelOrder = new OrderListViewModel
            {
                Orders = orders, 
                CurrentPage = page,
                TotalPages = totalPages,
                EventID = eventId
            };

            // ViewModel cho vé
            var viewModelTicket = new TicketListViewModel
            {
                Tickets = tickets,
                CurrentPage = page,
                TotalPages = totalPages1,
                EventID = eventId
            };

            // Nếu yêu cầu Ajax, trả về các PartialView riêng biệt cho mỗi phần
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (type == "orders")
                {
                    return PartialView("_OrderList", viewModelOrder);
                }
                else if (type == "tickets")
                {
                    return PartialView("_TicketList", viewModelTicket);
                }
            }

            // Trả về toàn bộ trang khi không phải yêu cầu Ajax
            var combinedViewModel = new CombinedViewModel
            {
                OrderViewModel = viewModelOrder,
                TicketViewModel = viewModelTicket,
                EventID = eventId
            };

            return View(combinedViewModel);
        }



        public IActionResult TicketDetails(int eventId)
        {
            var ticketDetails = _context.OrderDetail
             .Include(od => od.Ticket)
             .Where(od => od.Ticket.EventID == eventId)
             .ToList();


            return PartialView("_TicketDetails", ticketDetails);
        }




        // Xem chi tiết đơn hàng (Có thể dùng để kiểm tra trước khi duyệt)
        public IActionResult Details(int orderId)
        {
            var order = _context.Order.FirstOrDefault(o => o.OrderID == orderId);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // Duyệt đơn hàng (Thay đổi trạng thái)
        public IActionResult Approve(int orderId)
        {
            var order = _context.Order.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                // Thay đổi trạng thái đơn hàng thành Confirmed
                order.OrderStatus = OrderStatus.Confirmed;

                // Cập nhật vào cơ sở dữ liệu
                _context.SaveChanges();
            }

            // Quay lại danh sách đơn hàng
            return RedirectToAction("Index");
        }

        // Hủy đơn hàng (Thay đổi trạng thái thành Cancelled)
        public IActionResult Cancel(int orderId)
        {
            var order = _context.Order.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                // Thay đổi trạng thái đơn hàng thành Cancelled
                order.OrderStatus = OrderStatus.Cancelled;
                _context.SaveChanges();
            }

            // Quay lại danh sách đơn hàng
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> SendConfirmationEmails([FromBody] List<int> orderIds)
        {
            if (orderIds == null || orderIds.Count == 0)
            {
                return BadRequest("Không có đơn hàng nào được chọn.");
            }

            foreach (var orderId in orderIds)
            {
                // Lấy tất cả OrderDetails thuộc đơn này
                var orderDetails = await _context.OrderDetail
                    .Include(od => od.Order)
                    .Include(od => od.Ticket)
                    .Include(od => od.IssuedTickets)
                    .Where(od => od.OrderID == orderId)
                    .ToListAsync();

                // Nếu không có hoặc email trống => bỏ qua
                if (orderDetails == null || !orderDetails.Any() || string.IsNullOrEmpty(orderDetails[0].Order.Email))
                    continue;

                var order = orderDetails[0].Order;



                foreach (var orderDetail in orderDetails)
                {
                    foreach (var issued in orderDetail.IssuedTickets)
                    {
                        // Tìm sự kiện
                        var ev = await _context.Event
                            .Include(e => e.Locations)
                            .FirstOrDefaultAsync(e => e.EventID == orderDetail.Ticket.EventID);

                        if (ev == null || orderDetail.Ticket == null)
                            continue;

                        // Cập nhật số lượng vé đã bán
                        

                        // Tạo mã QR cho vé
                        var qrService = new QrCodeService();
                        var qrImage = qrService.GenerateQrImage(issued.TicketCode);

                        // Gửi email xác nhận
                        await _emailSender.SendEmailWithQrAsync(
                            toEmail: order.Email,
                            subject: "Thông tin vé sự kiện",
                            ticketCode: issued.TicketCode,
                            qrImage: qrImage,
                            eventName: ev.Title,
                            eventId: ev.EventID,
                            eventDate: orderDetail.Ticket.StartDate.ToString(),
                            ticketPrice: orderDetail.Ticket.Price,
                            quantity: orderDetail.Quantity,
                            totalAmount: order.TotalAmount,
                            eventLocation: $"{ev.Locations.First().Name}, {ev.Locations.First().FullAddress}, {ev.Locations.First().Ward}, {ev.Locations.First().District}, {ev.Locations.First().City}",
                            ticketName: orderDetail.Ticket.Description
                        );
                    }
                    orderDetail.Ticket.Sold += orderDetail.Quantity;

                    // Kiểm tra nếu số vé đã bán đủ số lượng thì đổi trạng thái vé thành hết vé
                    if (orderDetail.Ticket.Sold >= orderDetail.Ticket.Quantity)
                    {
                        orderDetail.Ticket.TrangThai = TicketStatus.HetVe;  // Thay đổi trạng thái vé
                    }
                }

                // Sau khi gửi xong hết mail cho đơn hàng này => đánh dấu đã gửi
                order.IsEmailSent = true;
            }

            // Lưu lại thay đổi trong cơ sở dữ liệu
            await _context.SaveChangesAsync();

            TempData["success"] = "Gửi email thành công";
            return Json(new { message = "Gửi email thành công!" });
        }



    }
}
