using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EFCoreCommerceAnalytics.ViewComponents.DashboardViewComponents
{
    public class _NavbarComponentPartial : ViewComponent
    {
        private readonly AppDbContext _context;

        public _NavbarComponentPartial(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {

            return View();
        }

    }
}
