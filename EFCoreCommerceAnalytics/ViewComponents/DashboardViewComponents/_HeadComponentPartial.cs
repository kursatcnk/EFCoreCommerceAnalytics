using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardViewComponents
{
    public class _HeadComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
