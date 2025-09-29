using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardViewComponents
{
    public class _DashboardScriptsComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
