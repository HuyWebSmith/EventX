using EventX.Models;
using EventX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EventX.Areas.Admin.Controllers
{
    [Area("Admin")] // Added Area attribute to specify the area
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var totalOrders = _context.Order.Count();

            var totalTicketsSold = _context.IssuedTickets.Count();

            var totalRevenue = _context.IssuedTickets
                .Include(it => it.OrderDetail)
                .ThenInclude(od => od.Ticket)
                .Sum(it => it.OrderDetail.Price - (it.OrderDetail.Ticket.Discount ?? 0));

            var totalUsers = _context.Users.Count();

            // Giả sử hoa hồng là 10% doanh thu
            decimal commissionRate = 0.1m; // 10%
            var totalCommission = totalRevenue * commissionRate;

            var model = new DashboardViewModel
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalTicketsSold = totalTicketsSold,
                TotalUsers = totalUsers,
                TotalCommission = totalCommission  // Bạn thêm thuộc tính này trong DashboardViewModel
            };

            return View(model);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
