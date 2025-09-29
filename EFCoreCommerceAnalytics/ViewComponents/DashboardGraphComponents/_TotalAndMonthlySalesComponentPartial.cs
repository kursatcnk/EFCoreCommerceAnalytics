using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Models;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardGraphComponents
{
    public class _TotalAndMonthlySalesComponentPartial:ViewComponent
    {
        private readonly AppDbContext _context;
        public _TotalAndMonthlySalesComponentPartial(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var result = _context.Orders.GroupBy(o => o.SaleStatus).Select(g => new OrderStatusChartViewModel
            {
                Status = g.Key,
                Count = g.Count()

            }).ToList();
            return View(result);
        }
    }
}
