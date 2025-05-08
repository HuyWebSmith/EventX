using EventX.Enums;
using EventX.Models;
using EventX.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EventX.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EventController : Controller
    {
        private readonly IEventRepository _eventRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;


        public EventController(ApplicationDbContext context, IEventRepository eventRepository, ICategoryRepository categoryRepository)
        {
            _context = context;
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;

        }


        public async Task<IActionResult> Index()
        {
            var events = await _context.Event
                .Include(e => e.Category)
                .Include(e => e.Tickets)        // Bao gồm vé
                .Include(e => e.Locations)      // Bao gồm địa điểm
                .ToListAsync();

            var categories = await _categoryRepository.GetAllAsync();

            foreach (var category in categories)
            {
                Console.WriteLine($"ID: {category.CategoryId} - Name: {category.Name}");
            }
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
            return View(events);
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
                .Include(e => e.PaymentInfos)     // Lấy thông tin thanh toán
                .Include(e => e.RedInvoices)      // Lấy thông tin hóa đơn đỏ
                .Include(e => e.Locations)        // Lấy thông tin địa điểm
                .FirstOrDefaultAsync(e => e.EventID == eventId);  // Tìm sự kiện theo eventId

            if (eventDetails != null)
            {
                eventDetails.Description = HttpUtility.HtmlDecode(eventDetails.Description);
            }

            // Trả về View với model chứa thông tin sự kiện
            return View(eventDetails);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int eventId, EventStatus status)
        {
            var eventEntity = await _context.Event.FindAsync(eventId);
            if (eventEntity == null)
            {
                return Json(new { success = false, message = "Sự kiện không tồn tại." });
            }

            // Chỉ cho phép chuyển sang "Đang diễn ra" nếu sự kiện đã được duyệt
            if (status == EventStatus.Ongoing)
            {
                if (eventEntity.Status != EventStatus.Approved)
                {
                    return Json(new { success = false, message = "Sự kiện chưa được duyệt, không thể chuyển sang Đang diễn ra." });
                }

                // Kiểm tra thời gian nếu cần thiết (đang diễn ra hay không)
                if (DateTime.Now < eventEntity.EventStartTime || DateTime.Now > eventEntity.EventEndTime)
                {
                    return Json(new { success = false, message = "Sự kiện chưa bắt đầu hoặc đã kết thúc." });
                }
            }

            // Cập nhật trạng thái sự kiện
            eventEntity.Status = status;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> SearchAndFilter(string keyword, string status, int? categoryId)
        {
            var query = _context.Event
                .Include(e => e.Category)
                .Include(e => e.Tickets)
                .Include(e => e.Locations)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(e => e.Title.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<EventStatus>(status, out var parsedStatus))
            {
                query = query.Where(e => e.Status == parsedStatus);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            var filteredEvents = await query.ToListAsync();
            return PartialView("_AdminEventTablePartial", filteredEvents);
        }

    }
}
