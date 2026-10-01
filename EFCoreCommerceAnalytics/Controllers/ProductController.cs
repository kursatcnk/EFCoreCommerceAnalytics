using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult ProductList(int page = 1)
        {
            const int pageSize = 10;
            var totalItems = _context.Products.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = _context.Products
                                   .Include(p => p.Category)
                                   .OrderBy(p => p.ProductId)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }

        // ----------- ADD -----------
        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewBag.Categories = _context.Categories
                                         .Select(c => new SelectListItem
                                         {
                                             Value = c.CategoryId.ToString(),
                                             Text = c.CategoryName
                                         }).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction("ProductList");
        }

        // ----------- DELETE -----------
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("ProductList");
        }

        // ----------- UPDATE -----------
        [HttpGet]
        public IActionResult UpdateProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categories
                                         .Select(c => new SelectListItem
                                         {
                                             Value = c.CategoryId.ToString(),
                                             Text = c.CategoryName
                                         }).ToList();
            return View(product);
        }

        [HttpPost]
        public IActionResult UpdateProduct(Product product)
        {       
            _context.Products.Update(product);
            _context.SaveChanges();
            return RedirectToAction("ProductList");
        }
    }
}
