using EventX.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventX.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class VoucherImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public VoucherImagesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.VoucherImages.OrderBy(v => v.Order).ToListAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(VoucherImages model)
        {
            if (model.ImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "uploads/vouchers");

                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                var fullPath = Path.Combine(filePath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                model.ImageUrl = "/uploads/vouchers/" + fileName;
            }
            else
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn hình ảnh.");
                return View(model);
            }

            model.CreatedDate = DateTime.Now;
            _context.VoucherImages.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
             var voucher = await _context.VoucherImages.FindAsync(id);
        if (voucher == null) return NotFound();

        _context.VoucherImages.Remove(voucher);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Toggle(int id)
        {
            var voucher = await _context.VoucherImages.FindAsync(id);
            if (voucher != null)
            {
                voucher.IsActive = !voucher.IsActive;
                _context.Update(voucher);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
