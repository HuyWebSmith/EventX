using EventX.Enums;
using EventX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


public class PendingOrderViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public PendingOrderViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = await _context.Order.CountAsync(o => o.OrderStatus == OrderStatus.Pending);
        return View(count);
    }
}
