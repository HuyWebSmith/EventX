using EventX.Enums;
using EventX.Models;
using EventX.Repositories;
using EventX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace EventX.Areas.Host.Controllers
{
    [Area("Host")]
    [Authorize(Roles = ",Host")]
    public class EventController : Controller
    {
        private readonly IEventRepository _eventRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly EventService _eventService;

        public EventController(EventService eventService,ApplicationDbContext context, IEventRepository eventRepository, ICategoryRepository categoryRepository)
        {
            _context = context;
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
            _eventService = eventService;

        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID người dùng hiện tại
            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Nếu không có người dùng, chuyển đến trang đăng nhập
            }
            

            // Lọc sự kiện chỉ của người dùng hiện tại
            var events = await _eventRepository.GetByIdAsync(userId);

            return View(events);
        }
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();  // Lấy danh mục từ repository
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");  // Gán SelectList vào ViewBag

            ViewBag.BusinessTypes = Enum.GetValues(typeof(BusinessType)).Cast<BusinessType>().ToList();
            ViewBag.TicketTypes = Enum.GetValues(typeof(TicketType)).Cast<TicketType>().ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> Add( Event model, 
            IFormFile organizerLogoFile,
            List<IFormFile> imageUrls,
            PaymentInfo paymentInfo,
            RedInvoice redInvoice,
            Location location,
            List<Ticket> tickets
            )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Lưu logo
                    if (organizerLogoFile != null)
                    {
                        var logoUrl = await SaveImage(organizerLogoFile);
                        model.OrganizerLogoUrl = logoUrl;
                    }

                    if (imageUrls != null && imageUrls.Count > 0)
                    {
                        var imageUrlsList = await SaveImages(imageUrls);
                        model.EventImages = imageUrlsList.Select(url => new EventImage { ImageURL = url }).ToList();
                    }
                   
                    // Tạo Event
                    var eventEntity = new Event
                    {
                        EventID = model.EventID,
                        OrganizerId = model.OrganizerId,
                        Title = model.Title,
                        Description = model.Description,
                        OrganizerName = model.OrganizerName,
                        OrganizerEmail = model.OrganizerEmail,
                        OrganizerInfo = model.OrganizerInfo,
                        CategoryId = model.CategoryId,
                        EventStartTime = model.EventStartTime,
                        EventEndTime = model.EventEndTime,
                        BuyerMessage = model.BuyerMessage,
                        OrganizerLogoUrl = model.OrganizerLogoUrl,
                        EventImages = model.EventImages,
                    };
                    // Lưu Event
                    _context.Event.Add(eventEntity);
                    await _context.SaveChangesAsync();  // Lưu Event vào cơ sở dữ liệu

                    

                    if (location != null)  // Kiểm tra RedInvoice có được truyền vào không
                    {
                        location.Event = eventEntity;
                        location.EventId = eventEntity.EventID;  // Gán EventId cho hóa đơn đỏ

                        // Thêm vào cơ sở dữ liệu
                        _context.Location.Add(location);
                        await _context.SaveChangesAsync();
                    }


                    foreach (var ticket in model.Tickets)
                    {
                        var ticketTypeString = ticket.Type.ToString();
                        if (string.IsNullOrEmpty(ticket.TicketCode))
                        {
                            ticket.TicketCode = $"TICKET-{ticketTypeString}{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                        }
                        ticket.Event = eventEntity; // Gán sự kiện cho vé
                        ticket.EventID = eventEntity.EventID;

                        eventEntity.Tickets.Add(ticket);
                    }
                    await _context.SaveChangesAsync();
                    // Lấy ID người tạo từ Identity
                    var creatorId = User.FindFirst(ClaimTypes.NameIdentifier).Value; // hoặc User.FindFirst(ClaimTypes.NameIdentifier)?.Value nếu dùng Claims

                    if (!string.IsNullOrEmpty(creatorId))
                    {
                        paymentInfo.EventId = eventEntity.EventID;  // Gán EventId cho PaymentInfo
                        paymentInfo.CreatorId = creatorId;
                        await _context.PaymentInfos.AddAsync(paymentInfo);
                        await _context.SaveChangesAsync();
                    }

                    if (redInvoice != null)  // Kiểm tra RedInvoice có được truyền vào không
                    {
                        redInvoice.Event = eventEntity; 
                        redInvoice.EventId = eventEntity.EventID;  // Gán EventId cho hóa đơn đỏ

                        // Thêm vào cơ sở dữ liệu
                        _context.RedInvoices.Add(redInvoice);
                        await _context.SaveChangesAsync();
                    }

                    // Quay lại trang Index
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    var inner = ex.InnerException?.Message;
                    var stackTrace = ex.StackTrace;
                    Console.WriteLine($"Error: {ex.Message} - Inner: {inner} - StackTrace: {stackTrace}");

                    return Json(new { success = false, message = $"Error: {ex.Message} - Inner: {inner}" });
                }
            }
            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepository.GetAllAsync();  // Lấy danh mục từ repository
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");  // Gán SelectList vào ViewBag

                ViewBag.BusinessTypes = Enum.GetValues(typeof(BusinessType)).Cast<BusinessType>().ToList();  // >>> Bổ sung dòng này
                ViewBag.TicketTypes = Enum.GetValues(typeof(TicketType)).Cast<TicketType>().ToList();

                // Xuất lỗi ra Console
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            return View(model);
            }

        private async Task<List<string>> SaveImages(List<IFormFile> images)
        {
            var imageUrls = new List<string>();
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var image in images)
            {
                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var savePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(savePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    imageUrls.Add("/images/" + fileName);
                }
            }

            return imageUrls;
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName); 
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối 
        }

        //Nhớ tạo folder imag
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
        public IActionResult Cancel(int id)
        {
            var eventItem = _context.Event.Find(id);
            if (eventItem != null)
            {
                eventItem.Status = EventStatus.Cancelled; // Thay đổi trạng thái sự kiện
                _context.SaveChanges(); // Lưu thay đổi
                return Json(new { success = true }); // Trả về kết quả thành công
            }
            return Json(new { success = false }); // Trường hợp không tìm thấy sự kiện
        }


        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            // Tìm sự kiện theo id
            var eventEntity = await _context.Event
                .Include(e => e.Category)
                .Include(e => e.EventImages)
                .Include(e => e.Tickets)
                .Include(e => e.Locations)
                .Include(e => e.PaymentInfos)  // Lấy thông tin thanh toán
                .Include(e => e.RedInvoices)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventEntity == null)
            {
                return NotFound();  // Nếu không tìm thấy sự kiện
            }

            var categories = await _categoryRepository.GetAllAsync();  // Lấy danh mục từ repository
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");  // Gán SelectList vào ViewBag
            ViewBag.BusinessTypes = Enum.GetValues(typeof(BusinessType)).Cast<BusinessType>().ToList();
            ViewBag.TicketTypes = Enum.GetValues(typeof(TicketType)).Cast<TicketType>().ToList();

            return View(eventEntity);  // Trả về view với sự kiện đã tìm thấy
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> Update(int id, Event model,
            IFormFile? organizerLogoFile,
            List<IFormFile> imageUrls,
            PaymentInfo paymentInfo,
            RedInvoice redInvoice,
            Location location,
            List<Ticket> tickets)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
                if (!string.IsNullOrEmpty(model.OrganizerLogoUrl))
                {
                    ModelState.Remove("OrganizerLogoFile"); // Bỏ lỗi nếu logo cũ tồn tại
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm sự kiện cần cập nhật
                    var eventEntity = await _context.Event.Include(e => e.Tickets).FirstOrDefaultAsync(e => e.EventID == id);
                    if (eventEntity == null)
                    {
                        return NotFound();  // Nếu không tìm thấy sự kiện
                    }

                    // Cập nhật thông tin sự kiện
                    eventEntity.Title = model.Title;
                    eventEntity.Description = model.Description;
                    eventEntity.OrganizerName = model.OrganizerName;
                    eventEntity.OrganizerEmail = model.OrganizerEmail;
                    eventEntity.OrganizerInfo = model.OrganizerInfo;
                    eventEntity.CategoryId = model.CategoryId;
                    eventEntity.EventStartTime = model.EventStartTime;
                    eventEntity.EventEndTime = model.EventEndTime;
                    eventEntity.BuyerMessage = model.BuyerMessage;
                    eventEntity.Status = EventStatus.Pending;

                    // Cập nhật logo (nếu có)
                    if (organizerLogoFile != null)
                    {
                        var newLogoUrl = await SaveImage(organizerLogoFile);
                        if (newLogoUrl != eventEntity.OrganizerLogoUrl)
                        {
                            eventEntity.OrganizerLogoUrl = newLogoUrl;
                        }
                    }


                    // Cập nhật hình ảnh sự kiện (nếu có)
                    if (imageUrls != null && imageUrls.Count > 0)
                    {
                        var newUrls = await SaveImages(imageUrls);
                        var currentUrls = eventEntity.EventImages.Select(i => i.ImageURL).ToList();

                        bool isSame = newUrls.OrderBy(x => x).SequenceEqual(currentUrls.OrderBy(x => x));
                        if (!isSame)
                        {
                            eventEntity.EventImages = newUrls.Select(url => new EventImage { ImageURL = url }).ToList();
                        }
                    }


                    // Cập nhật địa điểm
                    if (location != null)
                    {
                        var existingLocation = await _context.Location.FirstOrDefaultAsync(l => l.EventId == eventEntity.EventID);
                        if (existingLocation != null)
                        {
                            existingLocation.Name = location.Name;
                            existingLocation.FullAddress = location.FullAddress;
                            existingLocation.Ward = location.Ward;
                            existingLocation.District = location.District;
                            existingLocation.City = location.City;

                        }
                        else
                        {
                            // Nếu chưa có, thêm mới
                            location.EventId = eventEntity.EventID;
                            _context.Location.Add(location);
                        }
                    }


                    foreach (var ticket in model.Tickets)
                    {
                        var ticketTypeString = ticket.Type.ToString();

                        // Kiểm tra xem vé đã có trong hệ thống chưa, nếu có thì cập nhật, nếu không thì tạo mới
                        var existingTicket = eventEntity.Tickets.FirstOrDefault(t => t.TicketCode == ticket.TicketCode);

                        if (existingTicket != null)
                        {
                            // Cập nhật thông tin của vé nếu đã tồn tại
                            existingTicket.Type = ticket.Type;
                            existingTicket.Price = ticket.Price;
                            existingTicket.Quantity = ticket.Quantity;
                            existingTicket.TicketCode = ticket.TicketCode;
                            existingTicket.EventID = eventEntity.EventID;
                            existingTicket.Event = eventEntity;  // Cập nhật lại sự kiện nếu cần
                        }
                        else
                        {
                            // Nếu vé chưa tồn tại, tạo vé mới
                            if (string.IsNullOrEmpty(ticket.TicketCode))
                            {
                                ticket.TicketCode = $"TICKET-{ticketTypeString}{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                            }
                            ticket.Event = eventEntity; // Gán sự kiện cho vé
                            ticket.EventID = eventEntity.EventID;

                            eventEntity.Tickets.Add(ticket);  // Thêm vé mới vào danh sách
                        }
                    }

                    // Xóa vé nếu chúng không còn trong danh sách model.Tickets
                    var ticketsToDelete = eventEntity.Tickets
                        .Where(t => !model.Tickets.Any(m => m.TicketCode == t.TicketCode)) // tìm vé không có trong model
                        .ToList();

                    foreach (var ticket in ticketsToDelete)
                    {
                        eventEntity.Tickets.Remove(ticket); // Xóa vé khỏi danh sách
                    }


                    await _context.SaveChangesAsync(); // Make sure to save the changes after adding/updating tickets


                    if (paymentInfo != null)
                    {
                        var existingPayment = await _context.PaymentInfos.FirstOrDefaultAsync(p => p.EventId == eventEntity.EventID);
                        if (existingPayment != null)
                        {
                            existingPayment.AccountHolder = paymentInfo.AccountHolder;
                            existingPayment.AccountNumber = paymentInfo.AccountNumber;
                            existingPayment.BankName = paymentInfo.BankName;
                            existingPayment.Branch = paymentInfo.Branch;
                        }
                        else
                        {
                            paymentInfo.EventId = eventEntity.EventID;
                            _context.PaymentInfos.Add(paymentInfo);
                        }
                    }

                    if (redInvoice != null)
                    {
                        var existingInvoice = await _context.RedInvoices.FirstOrDefaultAsync(r => r.EventId == eventEntity.EventID);
                        if (existingInvoice != null)
                        {
                            existingInvoice.BusinessType = redInvoice.BusinessType;
                            existingInvoice.FullName = redInvoice.FullName;
                            existingInvoice.Address = redInvoice.Address;
                            existingInvoice.TaxCode = redInvoice.TaxCode;
                        }
                        else
                        {
                            redInvoice.EventId = eventEntity.EventID;
                            _context.RedInvoices.Add(redInvoice);
                        }
                    }




                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index"); // Quay lại trang danh sách sự kiện
                }
                catch (Exception ex)
                {
                    var inner = ex.InnerException?.Message;
                    var stackTrace = ex.StackTrace;
                    Console.WriteLine($"Error: {ex.Message} - Inner: {inner} - StackTrace: {stackTrace}");

                    return Json(new { success = false, message = $"Error: {ex.Message} - Inner: {inner}" });
                }
            }

            // Nếu không hợp lệ, quay lại trang cập nhật với lỗi
            var categories = await _categoryRepository.GetAllAsync();  // Lấy danh mục từ repository
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");  // Gán SelectList vào ViewBag
            ViewBag.BusinessTypes = Enum.GetValues(typeof(BusinessType)).Cast<BusinessType>().ToList();
            ViewBag.TicketTypes = Enum.GetValues(typeof(TicketType)).Cast<TicketType>().ToList();

            return View(model); // Quay lại view với dữ liệu không hợp lệ
        }

        [HttpGet]
        public IActionResult SearchAndFilter(string keyword, EventStatus? status)
        {
            var events = _context.Event
                .Include(e => e.EventImages)
                .Include(e => e.Locations)
                .Where(e =>
                    (string.IsNullOrEmpty(keyword) || e.Title.Contains(keyword)) &&
                    (!status.HasValue || e.Status == status.Value))
                .ToList();

            return PartialView("_EventCardPartial", events);
        }

        public IActionResult UpdateStatuses()
        {
            _eventService.UpdateEventStatus();
            return RedirectToAction("Index");
        }

    }
}
