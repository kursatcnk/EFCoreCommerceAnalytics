using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardViewComponents
{
    public class _ActivityComponentPartial : ViewComponent
    {
        private readonly Context.AppDbContext _context;
        public _ActivityComponentPartial(Context.AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {

            var activities = _context.Activities.ToList();
            return View(activities);
        }
    }
}
