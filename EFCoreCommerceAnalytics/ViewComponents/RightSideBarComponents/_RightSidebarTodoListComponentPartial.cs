using EFCoreCommerceAnalytics.Context;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents.RightSideBarComponents
{
    public class _RightSidebarTodoListComponentPartial : ViewComponent
    {
        private readonly AppDbContext _context;
        public _RightSidebarTodoListComponentPartial(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            // get me the last 5 todos from the database
            var todos = _context.ToDos.OrderByDescending(t => t.TodoId).Take(5).ToList();
            return View(todos);

        }
    }

}
