using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Models;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardViewComponents
{
    public class _SalesStatusComponentPartial:ViewComponent
    {
        private readonly AppDbContext _context;
        public _SalesStatusComponentPartial(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var data = _context.Customers.GroupBy(x => x.CustomerCity).Select(g => new CustomerCityChartViewModel
            {
                City = g.Key,
                Count = g.Count()
            }).ToList();
            return View(data);
        }
    }
}
