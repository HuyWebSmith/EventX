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
            // Upload ảnh
            if (slider.ImageFile != null)
            {
                var imagePath = "/images/sliders/" + Guid.NewGuid() + Path.GetExtension(slider.ImageFile.FileName);
                var imageFullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));
                using (var stream = new FileStream(imageFullPath, FileMode.Create))
                {
                    await slider.ImageFile.CopyToAsync(stream);
                }
                slider.ImageUrl = imagePath;
            }
            else
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn ảnh.");
                return View(slider);
            }

            // Upload video (nếu có)
            if (slider.VideoFile != null)
            {
                var videoPath = "/videos/sliders/" + Guid.NewGuid() + Path.GetExtension(slider.VideoFile.FileName);
                var videoFullPath = Path.Combine(_env.WebRootPath, videoPath.TrimStart('/'));

                // Tạo folder nếu chưa có
                var videoFolder = Path.Combine(_env.WebRootPath, "videos", "sliders");
                if (!Directory.Exists(videoFolder))
                    Directory.CreateDirectory(videoFolder);

                using (var stream = new FileStream(videoFullPath, FileMode.Create))
                {
                    await slider.VideoFile.CopyToAsync(stream);
                }
                slider.VideoUrl = videoPath;
            }

            if (!ModelState.IsValid)
            {
                return View(slider);
            }

            _context.Add(slider);
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
        public async Task<IActionResult> Edit(int id, Slider updatedSlider, IFormFile ImageFile, IFormFile VideoFile)
        {
            if (id != updatedSlider.Id)
            {
                return BadRequest();
            }

            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }

            slider.Title = updatedSlider.Title;
            slider.Link = updatedSlider.Link;
            slider.Description = updatedSlider.Description;
            slider.Order = updatedSlider.Order;
            slider.IsActive = updatedSlider.IsActive;

            // Upload ảnh mới
            if (ImageFile != null)
            {
                var imagePath = "/images/sliders/" + Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var imageFullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));
                using (var stream = new FileStream(imageFullPath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                slider.ImageUrl = imagePath;
            }

            // Upload video mới
            if (VideoFile != null)
            {
                var videoPath = "/videos/sliders/" + Guid.NewGuid() + Path.GetExtension(VideoFile.FileName);
                var videoFullPath = Path.Combine(_env.WebRootPath, videoPath.TrimStart('/'));

                var videoFolder = Path.Combine(_env.WebRootPath, "videos", "sliders");
                if (!Directory.Exists(videoFolder))
                    Directory.CreateDirectory(videoFolder);

                using (var stream = new FileStream(videoFullPath, FileMode.Create))
                {
                    await VideoFile.CopyToAsync(stream);
                }
                slider.VideoUrl = videoPath;
            }

            _context.Update(slider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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