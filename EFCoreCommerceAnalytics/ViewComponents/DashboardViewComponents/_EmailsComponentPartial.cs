using EFCoreCommerceAnalytics.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardViewComponents
{
    public class _EmailsComponentPartial : ViewComponent
    {
        private readonly AppDbContext _context;

        public _EmailsComponentPartial(AppDbContext context)
        {
            _context = context;
        }

        // ViewComponent çağrıldığında çalışacak method
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Son 5 mesajı tarihe göre sırala
            var lastFiveMessages = await _context.Messages
                                                .OrderByDescending(m => m.DateTime)
                                                .Take(5)
                                                .AsNoTracking()
                                                .ToListAsync();

            return View(lastFiveMessages);
        }
    }
}
