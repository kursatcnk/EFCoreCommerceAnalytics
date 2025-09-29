using EFCoreCommerceAnalytics.Context;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardViewComponents
{
    public class _ToDoComponentPartial:ViewComponent
    {
        private readonly AppDbContext _context;
        public _ToDoComponentPartial(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            //Id ye göre son 5 kaydı listeleme işlemi yapıyorum, kalabalık olmaması için 5 kaydın uyumlu olacağını düşündüm.
            var values = _context.ToDos.OrderByDescending(x=>x.TodoId).Take(5).ToList();
            return View(values);
        }
    }
}
