using System.Diagnostics;
using EFCoreCommerceAnalytics.Models;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.Controllers
{
    /// <summary>
    /// Hata sayfaları. Varsayılan MVC şablonundan kalan HomeController (Index/Privacy) kullanılmıyordu;
    /// geriye yalnızca hata sayfası ihtiyacı kaldı.
    /// </summary>
    public class ErrorController : AppController
    {
        [Route("/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index() =>
            View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        [Route("/Error/{statusCode:int}")]
        public IActionResult Status(int statusCode)
        {
            Response.StatusCode = statusCode;
            return View("NotFound");
        }
    }
}
