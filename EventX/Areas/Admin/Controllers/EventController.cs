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

                if (DateTime.Now < eventEntity.EventStartTime || DateTime.Now > eventEntity.EventEndTime)
                {
                    return Json(new { success = false, message = "Sự kiện chưa bắt đầu hoặc đã kết thúc." });
                }
            }

            // Cập nhật trạng thái sự kiện
            eventEntity.Status = status;
            await _context.SaveChangesAsync();

            Notification notification = null;

            switch (status)
            {
                case EventStatus.Pending:
                    notification = new Notification
                    {
                        UserId = eventEntity.OrganizerId,
                        Message = $"⌛ Sự kiện \"{eventEntity.Title}\" đang chờ duyệt.",
                        Type = NotificationType.ThongBao, // hoặc tùy chỉnh loại NotificationType
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    break;

                case EventStatus.Approved:
                    notification = new Notification
                    {
                        UserId = eventEntity.OrganizerId,
                        Message = $"🎉 Sự kiện \"{eventEntity.Title}\" đã được phê duyệt!",
                        Type = NotificationType.Duyet,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    break;

                case EventStatus.Rejected:
                    notification = new Notification
                    {
                        UserId = eventEntity.OrganizerId,
                        Message = $"❌ Sự kiện \"{eventEntity.Title}\" đã bị từ chối.",
                        Type = NotificationType.ThongBao,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    break;

                case EventStatus.Scheduled:
                    notification = new Notification
                    {
                        UserId = eventEntity.OrganizerId,
                        Message = $"📅 Sự kiện \"{eventEntity.Title}\" đã được lên lịch.",
                        Type = NotificationType.ThongBao,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    break;

                case EventStatus.Ongoing:
                    notification = new Notification
                    {
                        UserId = eventEntity.OrganizerId,
                        Message = $"🔔 Sự kiện \"{eventEntity.Title}\" đang diễn ra.",
                        Type = NotificationType.ThongBao,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    break;

                case EventStatus.Completed:
                    notification = new Notification
                    {
                        UserId = eventEntity.OrganizerId,
                        Message = $"✅ Sự kiện \"{eventEntity.Title}\" đã kết thúc.",
                        Type = NotificationType.ThongBao,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    break;

                case EventStatus.Cancelled:
                    notification = new Notification
                    {
                        UserId = eventEntity.OrganizerId,
                        Message = $"🚫 Sự kiện \"{eventEntity.Title}\" đã bị hủy.",
                        Type = NotificationType.ThongBao,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    break;
            }

            if (notification != null)
            {
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

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
