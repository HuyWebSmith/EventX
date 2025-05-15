using EventX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EventX.Areas.Host.Controllers
{
    [Area("Host")]
    public class CheckinController : Controller
    {
        public IActionResult Index(int eventId)
        {
            var model = new CheckinViewModel
            {
                EventId = eventId
            };
            return View(model);
        }
    }
}
