using EFCoreCommerceAnalytics.Context;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardViewComponents
{
    public class _StatisticsComponentPartial : ViewComponent
    {
        private readonly AppDbContext _context;
        public _StatisticsComponentPartial(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            ViewBag.TotalCustomerCount = _context.Customers.Count();
            ViewBag.TotalCategoryCount = _context.Categiores.Count();
            ViewBag.TotalProductCount = _context.Products.Count();
            ViewBag.AvarageCustomerBalance = Math.Round(_context.Customers.Average(x => x.CustomerBalance), 2);
            ViewBag.TotalSaleCount= _context.Orders.Count();
            ViewBag.TotalOrderedProductCount = _context.Orders.Sum(x => x.OrderCount);


            return View();
        }
    }
}
