using EFCoreCommerceAnalytics.Context;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardViewComponents
{
    public class _DailySalesComponentPartial:ViewComponent
    {
        private readonly AppDbContext _context;
        public _DailySalesComponentPartial(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var data= _context.ToDos.GroupBy(t => t.Priority)
                .Select(g => new Models.TodoStatusChartViewModel
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();
            return View(data);
        }
    }
}
