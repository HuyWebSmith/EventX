using EventX.Enums;
using EventX.Models;
using EventX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventX.Areas.Host.Controllers
{
    [Area("Host")]
    [Authorize(Roles = "Host")]// Chỉ Admin mới có quyền truy cập
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        //private readonly IEmailService _emailService; // Giả sử bạn đã có dịch vụ gửi email

        private const int PageSize = 10; // Số đơn hàng hiển thị trên mỗi trang

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
            //_emailService = emailService;
        }

        public IActionResult Index(int eventId, int page = 1)
        {
            // Số đơn hàng hiển thị trên mỗi trang
            int pageSize = 10;

            // Truy vấn đơn hàng có ít nhất 1 vé thuộc EventID cần tìm
            var query = _context.Order
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Ticket)
                .Where(o => o.OrderDetails.Any(od => od.Ticket != null && od.Ticket.EventID == eventId))
                .OrderByDescending(o => o.OrderID);

            // Tính tổng số đơn hàng và tổng số trang
            var totalOrders = query.Count();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            // Lấy danh sách đơn hàng với phân trang
            var orders = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new OrderListViewModel
            {
                Orders = orders,
                CurrentPage = page,
                TotalPages = totalPages,
                EventID = eventId
            };

            return View(viewModel);
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

        // Gửi email khi duyệt đơn hàng
        public IActionResult SendEmail(int orderId)
        {
            var order = _context.Order.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                // Tạo nội dung email
                var emailContent = GenerateEmailContent(order);
                // Gửi email cho khách hàng
                //_emailService.SendEmail(order.ApplicationUser.Email, "Xác nhận thanh toán", emailContent);
            }

            // Sau khi gửi email, quay lại danh sách đơn hàng
            return RedirectToAction("Index");
        }

        // Tạo nội dung email cho việc xác nhận thanh toán
        private string GenerateEmailContent(Order order)
        {
            var qrCode = GenerateQrCode(order);  // Giả sử bạn có phương thức tạo mã QR

            return $@"
                <h2>Đơn hàng đã được xác nhận</h2>
                <p>Mã đơn hàng: {order.OrderID}</p>
                <p>Tên khách hàng: {order.ApplicationUser.FullName}</p>
                <p>Tổng tiền: {order.TotalAmount}</p>
                <p>Trạng thái: Đã thanh toán</p>
                <img src='data:image/png;base64,{qrCode}' alt='QR Code'/>
            ";
        }

        // Giả sử có một phương thức tạo mã QR (bạn có thể dùng thư viện như QRCoder)
        private string GenerateQrCode(Order order)
        {
            // Tạo mã QR (Giả sử bạn có một phương thức mã hóa QR cho đơn hàng)
            return "base64-encoded-qrcode-string"; // Giả lập chuỗi mã QR base64
        }
    }
}
