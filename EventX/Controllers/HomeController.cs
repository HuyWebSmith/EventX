using EventX.Enums;
using EventX.Models;
using EventX.Repositories;
using EventX.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Web;

namespace EventX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEventRepository _eventRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(ILogger<HomeController> logger, 
            ApplicationDbContext context, 
            IEventRepository eventRepository, 
            ICategoryRepository categoryRepository,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var allEvents = await _eventRepository.GetAllAsync();
            var approvedEvents = allEvents
               .Where(e => e.Status == Enums.EventStatus.Approved || e.Status == Enums.EventStatus.Ongoing)
               .ToList();

            var sliders = await _context.Sliders
                .Where(s => s.IsActive)
                .OrderBy(s => s.Order)
                .ToListAsync();
            var vouchers = await _context.VoucherImages
                .Where(v => v.IsActive)
                .OrderBy(v => v.Order)
                .ToListAsync();

            // Lấy ngày hiện tại
            var today = DateTime.Today;

            // Tính thứ 6 cuối tuần (hoặc thứ 7, Chủ nhật tùy theo quy định)
            // Giả sử cuối tuần là thứ 7 và Chủ nhật
            var saturday = today.AddDays(DayOfWeek.Saturday - today.DayOfWeek);
            var sunday = saturday.AddDays(1);

            // Sự kiện cuối tuần: diễn ra trong khoảng thứ 7 hoặc chủ nhật tuần này
            var eventsThisWeekend = approvedEvents
                .Where(e => e.EventStartTime.Date >= saturday && e.EventStartTime.Date <= sunday)
                .ToList();

            // Sự kiện trong tháng này (bất kỳ ngày nào trong tháng hiện tại)
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var eventsThisMonth = approvedEvents
            .Where(e => e.EventStartTime.Date >= firstDayOfMonth && e.EventStartTime.Date <= lastDayOfMonth)
            .ToList();


            var viewModel = new HomeViewModel
            {
                Events = approvedEvents,
                Sliders = sliders,
                VoucherImages = vouchers,
                EventsThisWeekend = eventsThisWeekend,
                EventsThisMonth = eventsThisMonth
            };

            return View(viewModel);
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

        public async Task<IActionResult> Display(int? eventId)
        {
            if (eventId == null)
            {
                return NotFound();  // Nếu không tìm thấy eventId, trả về NotFound
            }
          

            var eventDetails = await _context.Event
            .Include(e => e.Category)
            .Include(e => e.Reviews)
             .ThenInclude(r => r.User)
            .Include(e => e.EventImages)
            .Include(e => e.Tickets)
            .Include(e => e.PaymentInfos)
            .Include(e => e.RedInvoices)
            .Include(e => e.Locations)
            .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventDetails == null)
            {
                return NotFound("Không tìm thấy sự kiện.");
            }

            // Sau khi load xong thì lọc vé
            if (eventDetails != null)
            {
                eventDetails.Tickets = eventDetails.Tickets
                    .Where(t => t.TrangThai != TicketStatus.NgungBan)
                    .ToList();
            }
            if (eventDetails != null)
            {
                eventDetails.Description = HttpUtility.HtmlDecode(eventDetails.Description);
            }

            // Trả về View với model chứa thông tin sự kiện
            return View(eventDetails);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<Event>()); // Hoặc View("Search", new List<Event>())
            }

            var results = _context.Event
                .Include(e => e.EventImages)
                .Where(e => e.Title.Contains(query) && (e.Status == EventStatus.Approved || e.Status == EventStatus.Ongoing || e.Status == EventStatus.Completed))
                .ToList();

            return View(results); // truyền danh sách kết quả xuống view
        }
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderBy(n => n.IsRead)              
                .ThenByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();

            return Json(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)  // lấy 5 thông báo mới nhất
                .ToListAsync();

            return Json(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var count = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return Json(new { count });
        }

        [HttpPost]
        public IActionResult MarkNotificationRead(int id)
        {
            var notification = _context.Notifications.Find(id);
            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            _context.SaveChanges();

            return Ok();
        }


        public class MarkReadRequest
        {
            public int Id { get; set; }
        }

        // GET: Giao diện đánh giá
        public async Task<IActionResult> Create(int eventId)
        {
            var user = await _userManager.GetUserAsync(User);

            bool hasTicket = _context.OrderDetail
                .Any(od => od.Ticket.EventID == eventId &&
                           od.IssuedTickets.Any(it => it.UserId == user.Id));

            if (!hasTicket)
                return Forbid();

            ViewBag.EventId = eventId;
            return View();

        }


        [HttpPost]
        public async Task<IActionResult> Create(Review review)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                review.UserId = user.Id;
            }
            else
            {
                review.UserId = null;
            }
            review.CreatedAt = DateTime.Now;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Display", "Home", new { eventId = review.EventId });
        }
    }
}
