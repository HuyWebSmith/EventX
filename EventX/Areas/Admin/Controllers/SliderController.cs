using EventX.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SliderController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var sliders = _context.Sliders.ToList();
            return View(sliders);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(SliderModel model)
        {
            if (ModelState.IsValid)
            {

                if (model.ImageUpLoad != null && model.ImageUpLoad.Length > 0)
                {
                    // Lưu ảnh và nhận tên tệp
                    var fileName = await SaveImage(model.ImageUpLoad);

                    // Cập nhật đường dẫn ảnh
                    model.ImagePath = "/images/sliders/" + fileName;
                }
                else
                {
                    // Nếu không có ảnh tải lên, thêm lỗi vào ModelState
                    ModelState.AddModelError("ImageUpLoad", "Ảnh Slider là bắt buộc.");
                    return View(model);
                }
                
                var slider = new SliderModel
                {
                    Title = model.Title,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    ImagePath = model.ImagePath,
                    Link = model.Link,
                    CreatedAt = DateTime.Now
                };
                _context.Sliders.Add(slider);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
                 return View(model);
            }
               
            return View(model);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images/sliders", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/sliders/" + image.FileName; // Trả về đường dẫn tương đối 
        }

    }
}
