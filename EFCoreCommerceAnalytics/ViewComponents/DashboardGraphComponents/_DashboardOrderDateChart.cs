using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Models;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardGraphComponents
{
    public class _DashboardOrderDateChart : ViewComponent
    {
        private readonly AppDbContext _context;
        public _DashboardOrderDateChart(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var data = _context.Orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    RawDate = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.RawDate)
                .ToList()
                .Select(x => new OrderDateViewModel
                {
                    Date = x.RawDate.ToString("yyyy-MM-dd"),
                    Count = x.Count
                }).ToList();
            return View(data);
        }
    }
}