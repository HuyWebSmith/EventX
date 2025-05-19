using EventX.Enums;
using EventX.Models;
using EventX.Repositories;
using EventX.ViewModels;
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
        public HomeController(ILogger<HomeController> logger, 
            ApplicationDbContext context, 
            IEventRepository eventRepository, 
            ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _context = context;
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
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

            var viewModel = new HomeViewModel
            {
                Events = approvedEvents,
                Sliders = sliders
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
            .Include(e => e.EventImages)
            .Include(e => e.Tickets)
            .Include(e => e.PaymentInfos)
            .Include(e => e.RedInvoices)
            .Include(e => e.Locations)
            .FirstOrDefaultAsync(e => e.EventID == eventId);

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
                .Where(e => e.Title.Contains(query) && (e.Status == EventStatus.Approved || e.Status == EventStatus.Ongoing))
                .ToList();

            return View(results); // truyền danh sách kết quả xuống view
        }

    }
}
