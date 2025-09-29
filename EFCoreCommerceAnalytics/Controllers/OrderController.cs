using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // -------- LIST + AJAX (Asenkron) --------
        public async Task<IActionResult> OrderList(string search = "", int page = 1)
        {
            int pageSize = 10;
            var query = _context.Orders
                                .Include(o => o.Customer)
                                .Include(o => o.Product)
                                .Where(o => o.SaleStatus != "İptal Edildi") // sadece iptal olmayanlar
                                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(o =>
                    o.Customer.CustomerFirstName.ToLower().Contains(search) ||
                    o.Customer.CustomerLastName.ToLower().Contains(search) ||
                    o.Product.ProductName.ToLower().Contains(search) ||
                    o.SaleStatus.ToLower().Contains(search));
            }

            int totalCount = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;

            var orders = await query
                         .OrderBy(o => o.OrderId)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize)
                         .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var html = string.Join("", orders.Select(o => $@"
                    <tr>
                        <td>{o.OrderId}</td>
                        <td>{o.Product?.ProductName}</td>
                        <td>{o.Customer?.CustomerFirstName} {o.Customer?.CustomerLastName}</td>
                        <td>{o.OrderCount}</td>
                        <td>{o.UnitPrice:C}</td>
                        <td>{o.TotalPrice:C}</td>
                        <td>{o.OrderDate:dd.MM.yyyy}</td>
                        <td>{o.SaleStatus}</td>
                        <td>
                            <a href='/Order/CancelOrder/{o.OrderId}' class='btn btn-outline-danger btn-sm'>İptal Et</a>
                            <a href='/Order/UpdateOrder/{o.OrderId}' class='btn btn-outline-success btn-sm'>İşlem Yap</a>
                        </td>
                    </tr>
                "));
                return Content(html, "text/html");
            }

            return View(orders);
        }

        // -------- TESLİM EDİLDİ VE İPTAL OLMAYANLAR (PAGINATION) --------
        [HttpGet]
        public async Task<IActionResult> DeliveredAndActiveOrders(int page = 1)
        {
            int pageSize = 10;

            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Product)
                .Where(o => o.SaleStatus != "İptal Edildi") // sadece iptal olmayanlar
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            int totalCount = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(orders);
        }


        // -------- CREATE --------
        [HttpGet]
        public async Task<IActionResult> CreateOrder()
        {
            ViewBag.Customers = await _context.Customers.ToListAsync();
            ViewBag.Products = await _context.Products.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            order.UnitPrice = await _context.Products
                                      .Where(p => p.ProductId == order.ProductId)
                                      .Select(p => p.ProductPrice)
                                      .FirstOrDefaultAsync();

            order.TotalPrice = order.UnitPrice * order.OrderCount;
            order.OrderDate = DateTime.Now;

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("OrderList");
        }

        // -------- UPDATE --------
        [HttpGet]
        public async Task<IActionResult> UpdateOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            ViewBag.Customers = await _context.Customers.ToListAsync();
            ViewBag.Products = await _context.Products.ToListAsync();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(Order order)
        {
            var existing = await _context.Orders.FindAsync(order.OrderId);
            if (existing == null) return NotFound();

            existing.CustomerId = order.CustomerId;
            existing.ProductId = order.ProductId;
            existing.OrderCount = order.OrderCount;
            existing.UnitPrice = await _context.Products
                                         .Where(p => p.ProductId == order.ProductId)
                                         .Select(p => p.ProductPrice)
                                         .FirstOrDefaultAsync();
            existing.TotalPrice = existing.UnitPrice * order.OrderCount;
            existing.SaleStatus = order.SaleStatus;

            await _context.SaveChangesAsync();
            return RedirectToAction("OrderList");
        }

        // -------- CANCEL --------
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.SaleStatus = "İptal Edildi";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("OrderList");
        }

        // -------- CANCELED ORDERS --------
        public async Task<IActionResult> CanceledOrders()
        {
            var canceledOrders = await _context.Orders
                                         .Include(o => o.Customer)
                                         .Include(o => o.Product)
                                         .Where(o => o.SaleStatus == "İptal Edildi")
                                         .ToListAsync();
            return View(canceledOrders);
        }

        // -------- SEARCH FOR MODALS --------
        public async Task<IActionResult> SearchCustomers(string term)
        {
            var customers = await _context.Customers
                                    .Where(c => c.CustomerFirstName.ToLower().Contains(term.ToLower()) ||
                                                c.CustomerLastName.ToLower().Contains(term.ToLower()))
                                    .Select(c => new
                                    {
                                        c.CustomerId,
                                        c.CustomerFirstName,
                                        c.CustomerLastName
                                    }).ToListAsync();

            return Json(customers);
        }

        public async Task<IActionResult> SearchProducts(string term)
        {
            var products = await _context.Products
                                   .Where(p => p.ProductName.ToLower().Contains(term.ToLower()))
                                   .Select(p => new
                                   {
                                       p.ProductId,
                                       p.ProductName,
                                       p.ProductPrice
                                   }).ToListAsync();

            return Json(products);
        }
    }
}
