using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.Controllers
{
    /// <summary>Sayfaların içeriği view component'lerden geliyor; burada sadece sayfa seçiliyor.</summary>
    public class DashboardController : AppController
    {
        public IActionResult Index() => View();

        public IActionResult Statistics() => View();
    }
}
