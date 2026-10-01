using EFCoreCommerceAnalytics.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCoreCommerceAnalytics.ViewComponents.StatisticsViewComponents
{
    public class _StastisticWidgetComponentPartial : ViewComponent
    {
        private readonly AppDbContext _context;

        public _StastisticWidgetComponentPartial(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // ===========================
            // VERİLERİ HESAPLADIĞIM YER
            // ===========================

            int categoryCount = _context.Categories.Count();
            decimal mostExpensiveProduct = _context.Products.Max(p => p.ProductPrice);
            decimal mostCheapProduct = _context.Products.Min(p => p.ProductPrice);
            int totalOrderCount = _context.Orders.Count();
            int todaysOrderCount = _context.Orders
                .Where(o => o.OrderDate.Date == DateTime.Now.Date)
                .Count();
            int totalProductCount = _context.Products.Count();

            var mostPopularProduct = _context.Orders
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalCount = g.Sum(x => x.OrderCount)
                })
                .OrderByDescending(x => x.TotalCount)
                .Join(_context.Products,
                      g => g.ProductId,
                      p => p.ProductId,
                      (g, p) => new { p.ProductName, g.TotalCount })
                .FirstOrDefault();

            string mostPopularProductName = mostPopularProduct?.ProductName ?? "Yok";
            int mostPopularProductTotalOrders = mostPopularProduct?.TotalCount ?? 0;

            int totalStockCount = _context.Products.Sum(p => p.ProductStock);
            int totalStockUsedInOrders = _context.Orders.Sum(o => o.OrderCount);
            decimal averageProductPrice = _context.Products.Average(p => p.ProductPrice);

            var topRevenueCategory = _context.Categories
                .Select(c => new
                {
                    c.CategoryName,
                    TotalRevenue = c.Products
                        .SelectMany(p => p.Orders)
                        .Sum(o => o.TotalPrice)
                })
                .OrderByDescending(c => c.TotalRevenue)
                .Select(c => c.CategoryName)
                .FirstOrDefault();

            int topRevenueCategoryOrders = _context.Categories
                .Select(c => new
                {
                    TotalRevenue = c.Products
                        .SelectMany(p => p.Orders)
                        .Sum(o => o.TotalPrice),
                    TotalOrders = c.Products
                        .SelectMany(p => p.Orders)
                        .Sum(o => o.OrderCount)
                })
                .OrderByDescending(c => c.TotalRevenue)
                .Select(c => c.TotalOrders)
                .FirstOrDefault();

            // ===========================
            // YENİ EKLENEN SORGULAR
            // ===========================

            // En çok stoğu olan ürün
            var highestStockProduct = _context.Products
                .OrderByDescending(p => p.ProductStock)
                .Select(p => new { p.ProductName, p.ProductStock })
                .FirstOrDefault();
            string highestStockProductName = highestStockProduct?.ProductName ?? "Yok";
            int highestStockAmount = highestStockProduct?.ProductStock ?? 0;

            // 5000 TL’den ucuz ürün sayısı
            int productsBelow5000Count = _context.Products
                .Count(p => p.ProductPrice < 5000);

            // Stoğu en az olan ürün
            var lowestStockProduct = _context.Products
                .OrderBy(p => p.ProductStock)
                .Select(p => new { p.ProductName, p.ProductStock })
                .FirstOrDefault();
            string lowestStockProductName = lowestStockProduct?.ProductName ?? "Yok";
            int lowestStockAmount = lowestStockProduct?.ProductStock ?? 0;

            // ===========================
            // VIEWBAG ATAMALARI
            // ===========================
            ViewBag.categoryCount = categoryCount;
            ViewBag.mostExpensiveProduct = mostExpensiveProduct;
            ViewBag.mostCheapProduct = mostCheapProduct;
            ViewBag.totalOrderCount = totalOrderCount;
            ViewBag.todaysOrderCount = todaysOrderCount;
            ViewBag.totalProductCount = totalProductCount;
            ViewBag.mostPopularProduct = mostPopularProductName;
            ViewBag.mostPopularProductTotalOrders = mostPopularProductTotalOrders;
            ViewBag.totalStockCount = totalStockCount;
            ViewBag.totalStockUsedInOrders = totalStockUsedInOrders;
            ViewBag.averageProductPrice = averageProductPrice;
            ViewBag.topRevenueCategory = topRevenueCategory;
            ViewBag.TopRevenueCategoryOrders = topRevenueCategoryOrders;

            // Yeni ViewBag atamaları
            ViewBag.HighestStockProductName = highestStockProductName;
            ViewBag.HighestStockAmount = highestStockAmount;
            ViewBag.ProductsBelow5000Count = productsBelow5000Count;
            ViewBag.LowestStockProductName = lowestStockProductName;
            ViewBag.LowestStockAmount = lowestStockAmount;

            return View();
        }
    }
}
