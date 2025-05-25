using EventX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventX.Component
{
    public class PendingEventViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public PendingEventViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var count = await _context.Event.CountAsync(e => e.Status == Enums.EventStatus.Pending);
            return View(count);
        }
    }
}
