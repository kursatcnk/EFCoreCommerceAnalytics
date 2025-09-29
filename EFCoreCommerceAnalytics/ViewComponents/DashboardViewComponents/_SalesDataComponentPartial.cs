using EFCoreCommerceAnalytics.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardViewComponents
{
    public class _SalesDataComponentPartial : ViewComponent
    {
        private readonly AppDbContext _context;
        public _SalesDataComponentPartial(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var latest5Sales = _context.Orders
        .Include(o => o.Customer) // Müşteri ilişkisi
        .Include(o => o.Product)  // Ürün ilişkisi
        .OrderByDescending(o => o.OrderDate) // En yeni tarih önce
        .Take(5)
        .ToList();

            return View(latest5Sales);

        }
    }
}
