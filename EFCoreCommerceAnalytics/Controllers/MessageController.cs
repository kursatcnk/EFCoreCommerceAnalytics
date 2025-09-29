using EFCoreCommerceAnalytics.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class MessageController : Controller
    {
        private readonly AppDbContext _context;
        public MessageController(AppDbContext context)
        {
            _context = context;
        }

        // Mesajları sayfalı (pagination) listeler
        public async Task<IActionResult> MessageList(int page = 1)
        {
            const int pageSize = 10; // her sayfada 10 kayıt

            var query = _context.Messages
                                .OrderByDescending(m => m.DateTime)
                                .AsQueryable();

            int totalCount = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;

            var messages = await query
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize).AsNoTracking()
                                .ToListAsync();

            return View(messages);
        }
        
    }
}
