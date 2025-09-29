using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;

        // Veri tabanı işlemleri için context'i alıyoruz
        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        // Müşteri listesini sayfalı ve aramalı getirir, AJAX isteğinde sadece tablo satırlarını döner
        public async Task<IActionResult> CustomerList(string search = "", int page = 1)
        {
            int pageSize = 10;
            var query = _context.Customers.AsQueryable();

            // Arama filtresi uygula
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    c.CustomerFirstName.ToLower().Contains(search) ||
                    c.CustomerLastName.ToLower().Contains(search));
            }

            // Toplam sayfa ve sayfalama bilgisi
            int totalCount = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;

            var customers = await query
                            .OrderBy(c => c.CustomerId)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            // AJAX isteği geldiğinde tablo satırlarını döner
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var html = string.Join("", customers.Select(c => $@"
                    <tr>
                        <td>{c.CustomerId}</td>
                        <td>{c.CustomerFirstName}</td>
                        <td>{c.CustomerLastName}</td>
                        <td>{c.CustomerCity}</td>
                        <td>{c.CustomerDistrict}</td>
                        <td>{c.CustomerBalance:C}</td>
                        <td>{(!string.IsNullOrEmpty(c.CustomerImageUrl) ? $"<img src='{c.CustomerImageUrl}' style='width:50px;height:50px;object-fit:cover;' />" : "")}</td>
                        <td>
                            <a href='/Customer/DeleteCustomer/{c.CustomerId}' class='btn btn-outline-danger btn-sm'>Sil</a>
                            <a href='/Customer/UpdateCustomer/{c.CustomerId}' class='btn btn-outline-success btn-sm'>Güncelle</a>
                        </td>
                    </tr>
                "));
                return Content(html, "text/html");
            }

            return View(customers);
        }

        // En çok sipariş alan 3 şehir ve şehirlerde en çok sipariş veren müşterileri getirir
        [HttpGet]
        public async Task<IActionResult> TopCitiesWithTopCustomers()
        {
            // Şehir bazlı toplam sipariş sayısı
            var topCities = await _context.Orders
                .GroupBy(o => o.Customer.CustomerCity)
                .Select(g => new { City = g.Key, TotalOrders = g.Count() })
                .OrderByDescending(g => g.TotalOrders)
                .Take(3)
                .ToListAsync();

            var result = new List<dynamic>();

            foreach (var city in topCities)
            {
                // Şehirde en çok sipariş veren müşteri
                var topCustomer = await _context.Orders
                    .Where(o => o.Customer.CustomerCity == city.City)
                    .GroupBy(o => new { o.CustomerId, o.Customer.CustomerFirstName, o.Customer.CustomerLastName })
                    .Select(g => new { CustomerId = g.Key.CustomerId, Name = $"{g.Key.CustomerFirstName} {g.Key.CustomerLastName}", OrdersCount = g.Count() })
                    .OrderByDescending(g => g.OrdersCount)
                    .FirstOrDefaultAsync();

                result.Add(new
                {
                    city.City,
                    city.TotalOrders,
                    topCustomer.CustomerId,
                    topCustomer.Name,
                    topCustomer.OrdersCount
                });
            }

            return View(result);
        }

        // Yeni müşteri oluşturma formunu döner
        [HttpGet]
        public IActionResult CreateCustomer() => View();

        // Yeni müşteri oluşturur ve kaydeder
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(Customer customer)
        {
            await _context.Customers.AddAsync(customer); // Müşteri ekle
            await _context.SaveChangesAsync(); // Değişiklikleri kaydet
            return RedirectToAction("CustomerList"); // Listeye yönlendir
        }

        // Belirtilen müşteriyi siler
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("CustomerList");
        }

        // Müşteri güncelleme formunu döner
        [HttpGet]
        public async Task<IActionResult> UpdateCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // Müşteri bilgilerini günceller
        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(Customer customer)
        {
            var existingCustomer = await _context.Customers.FindAsync(customer.CustomerId);
            if (existingCustomer == null) return NotFound();

            existingCustomer.CustomerFirstName = customer.CustomerFirstName;
            existingCustomer.CustomerLastName = customer.CustomerLastName;
            existingCustomer.CustomerCity = customer.CustomerCity;
            existingCustomer.CustomerDistrict = customer.CustomerDistrict;
            existingCustomer.CustomerBalance = customer.CustomerBalance;
            existingCustomer.CustomerImageUrl = customer.CustomerImageUrl;

            await _context.SaveChangesAsync();
            return RedirectToAction("CustomerList");
        }

        // Bakiyesi 1000’in üzerinde olan müşterileri sayfalı listeler
        [HttpGet]
        public async Task<IActionResult> CustomersNormalBalance(int page = 1)
        {
            int pageSize = 10;
            var allCustomers = await _context.Customers.ToListAsync();
            var highBalanceCustomers = allCustomers.Where(c => c.CustomerBalance < 1000).ToList();
            var normalCustomers = allCustomers.Except(highBalanceCustomers)
                .OrderBy(c => c.CustomerId)
                .ToList();

            int totalCount = normalCustomers.Count;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;

            var pageCustomers = normalCustomers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return View(pageCustomers);
        }
    }
}
