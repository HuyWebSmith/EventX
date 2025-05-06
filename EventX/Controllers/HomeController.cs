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
                .Include(e => e.Category)         // Lấy thông tin thể loại sự kiện
                .Include(e => e.EventImages)      // Lấy danh sách hình ảnh sự kiện
                .Include(e => e.Tickets)          // Lấy danh sách vé sự kiện
                .Include(e => e.Locations)        // Lấy thông tin địa điểm
                .FirstOrDefaultAsync(e => e.EventID == eventId);  // Tìm sự kiện theo eventId

            if (eventDetails != null)
            {
                eventDetails.Description = HttpUtility.HtmlDecode(eventDetails.Description);
            }

            // Trả về View với model chứa thông tin sự kiện
            return View(eventDetails);
        }
    }
}
