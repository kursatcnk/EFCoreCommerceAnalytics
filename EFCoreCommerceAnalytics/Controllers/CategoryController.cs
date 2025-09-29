using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // AJAX ve arama desteği
        public async Task<IActionResult> CategoryList(string search = "", int page = 1)
        {
            int pageSize = 10;
            var query = _context.Categiores.AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(c => c.CategoryName.ToLower().Contains(search));
            }

            // Toplam sayfa bilgisi
            int totalCount = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;

            var categories = await query
                .OrderBy(c => c.CategoryId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // AJAX isteğinde yalnızca satır HTML’i dön
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var html = string.Join("", categories.Select(c => $@"
                    <tr>
                        <td>{c.CategoryId}</td>
                        <td>{c.CategoryName}</td>
                        <td>{(c.Status ? "<label class='badge badge-success badge-pill'>Aktif</label>"
                                       : "<label class='badge badge-danger badge-pill'>Pasif</label>")}</td>
                        <td><a href='/Category/DeleteCategory/{c.CategoryId}' class='btn btn-outline-danger btn-sm'>Sil</a></td>
                        <td><a href='/Category/UpdateCategory/{c.CategoryId}' class='btn btn-outline-success btn-sm'>Güncelle</a></td>
                    </tr>
                "));
                return Content(html, "text/html");
            }

            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory() => View();

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            category.Status = true;
            _context.Categiores.Add(category);
            _context.SaveChanges();
            return RedirectToAction("CategoryList");
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categiores.Find(id);
            if (category != null)
            {
                _context.Categiores.Remove(category);
                _context.SaveChanges();
            }
            return RedirectToAction("CategoryList");
        }

        [HttpGet]
        public IActionResult UpdateCategory(int id)
        {
            var category = _context.Categiores.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult UpdateCategory(Category category)
        {
            var existing = _context.Categiores.Find(category.CategoryId);
            if (existing == null) return NotFound();

            existing.CategoryName = category.CategoryName;
            existing.Status = category.Status;
            _context.SaveChanges();
            return RedirectToAction("CategoryList");
        }
    }
}
