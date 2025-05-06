using EventX.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EventX.Areas.Admin.Controllers
{
    [Area("Admin")] // Added Area attribute to specify the area
    [Authorize(Roles = "Admin")]
    public class SlidersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlidersController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var sliders = await _context.Sliders.OrderBy(s => s.Order).ToListAsync();
            return View(sliders);
        }

        public IActionResult Create()
        {
            return View(); // Trả về view tạo slider
        }

        [HttpPost]
        public async Task<IActionResult> Create(Slider slider)
        {
            if (slider.ImageFile != null)
            {
                var path = "/images/sliders/" + Guid.NewGuid() + Path.GetExtension(slider.ImageFile.FileName);
                var fullPath = Path.Combine(_env.WebRootPath, path.TrimStart('/'));
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await slider.ImageFile.CopyToAsync(stream);
                }
                slider.ImageUrl = path;
            }
            else
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn ảnh.");
                return View(slider);
            }
            if (!ModelState.IsValid)
            {
                // Ghi ra các lỗi trong ModelState để kiểm tra
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            var sliders = new Slider
            {
                Id = slider.Id,
                Title = slider.Title,
                Description = slider.Description,
                Link = slider.Link,
                Order = slider.Order,
                ImageUrl = slider.ImageUrl,
                IsActive = slider.IsActive
            };

            _context.Add(sliders);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound(); // Nếu không tìm thấy Slider
            }
            return View(slider); // Trả về view với Slider đã tìm thấy
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, Slider updatedSlider, IFormFile ImageFile)
        {
            if (id != updatedSlider.Id)
            {
                return BadRequest(); // Kiểm tra nếu id trong URL và id trong dữ liệu không khớp
            }

            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();  // Nếu không tìm thấy Slider
            }

            // Cập nhật các thông tin của Slider
            slider.Title = updatedSlider.Title;
            slider.Link = updatedSlider.Link;
            slider.Description = updatedSlider.Description;
            slider.Order = updatedSlider.Order;
            slider.IsActive = updatedSlider.IsActive;

            // Nếu người dùng tải lên ảnh mới, cập nhật hình ảnh
            if (ImageFile != null)
            {
                var path = "/images/sliders/" + Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var fullPath = Path.Combine(_env.WebRootPath, path.TrimStart('/'));
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                slider.ImageUrl = path; // Cập nhật URL hình ảnh
            }
                _context.Update(slider); // Cập nhật đối tượng trong context
                await _context.SaveChangesAsync(); // Lưu thay đổi vào database
                return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách Slider  
        }


        public async Task<IActionResult> Delete(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider != null)
            {
                _context.Sliders.Remove(slider);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Toggle(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider != null)
            {
                slider.IsActive = !slider.IsActive;
                _context.Update(slider);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }

}