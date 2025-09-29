using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents._LayoutComponents
{
    public class _LayoutScriptsComponentPartial: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
