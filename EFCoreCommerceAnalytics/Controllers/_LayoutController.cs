using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class _LayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
