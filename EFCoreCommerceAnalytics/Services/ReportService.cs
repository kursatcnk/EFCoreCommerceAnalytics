using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using EFCoreCommerceAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Services
{
    /// <summary>Dashboard ve istatistik sayfasındaki bütün hesaplamalar.</summary>
    public interface IReportService
    {
        Task<DashboardSummary> GetSummaryAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ChartPoint>> GetOrdersByStatusAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ChartPoint>> GetDailyOrderCountsAsync(int days, CancellationToken ct = default);
        Task<IReadOnlyList<ChartPoint>> GetCustomersByCityAsync(int top, CancellationToken ct = default);
        Task<IReadOnlyList<ChartPoint>> GetToDosByPriorityAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Activity>> GetActivitiesAsync(CancellationToken ct = default);
        Task<StoreStatistics> GetStoreStatisticsAsync(decimal priceThreshold = 5000m, CancellationToken ct = default);
    }

    public sealed class ReportService : IReportService
    {
        private const string Unspecified = "Belirtilmemiş";

        private readonly AppDbContext _db;
        private readonly TimeProvider _clock;

        public ReportService(AppDbContext db, TimeProvider clock)
        {
            _db = db;
            _clock = clock;
        }

        public async Task<DashboardSummary> GetSummaryAsync(CancellationToken ct = default)
        {
            // Average boş tabloda hata verir; nullable'a çevirince boş tabloda null döner.
            var averageBalance = await _db.Customers.AverageAsync(c => (decimal?)c.CustomerBalance, ct);

            return new DashboardSummary(
                CustomerCount: await _db.Customers.CountAsync(ct),
                CategoryCount: await _db.Categories.CountAsync(ct),
                ProductCount: await _db.Products.CountAsync(ct),
                AverageCustomerBalance: averageBalance is null ? null : Math.Round(averageBalance.Value, 2),
                OrderCount: await _db.Orders.CountAsync(ct),
                OrderedItemCount: await _db.Orders.SumAsync(o => o.OrderCount, ct));
        }

        public async Task<IReadOnlyList<ChartPoint>> GetOrdersByStatusAsync(CancellationToken ct = default)
        {
            var rows = await _db.Orders.AsNoTracking()
                .GroupBy(o => o.SaleStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            // Durumlar akış sırasıyla gösterilsin; listede olmayan eski değerler sona eklensin.
            return rows
                .OrderBy(r => r.Status is null ? int.MaxValue : IndexOf(OrderStatuses.All, r.Status))
                .Select(r => new ChartPoint(r.Status ?? Unspecified, r.Count))
                .ToList();
        }

        /// <summary>Son N günün günlük sipariş sayıları; sipariş olmayan günler de 0 olarak listede yer alır.</summary>
        public async Task<IReadOnlyList<ChartPoint>> GetDailyOrderCountsAsync(int days, CancellationToken ct = default)
        {
            var today = _clock.GetLocalNow().Date;
            var from = today.AddDays(-(days - 1));

            var counts = await _db.Orders.AsNoTracking()
                .Where(o => o.OrderDate >= from)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Day, x => x.Count, ct);

            return Enumerable.Range(0, days)
                .Select(i => from.AddDays(i))
                .Select(day => new ChartPoint(day.ToString("dd.MM"), counts.GetValueOrDefault(day)))
                .ToList();
        }

        public async Task<IReadOnlyList<ChartPoint>> GetCustomersByCityAsync(int top, CancellationToken ct = default)
        {
            var rows = await _db.Customers.AsNoTracking()
                .GroupBy(c => c.CustomerCity)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(ct);

            // Çok şehir varsa grafik okunmaz hâle geliyordu; ilk N şehir gösterilip kalanı "Diğer" olarak toplanıyor.
            var points = rows.Take(top).Select(r => new ChartPoint(r.City, r.Count)).ToList();
            var rest = rows.Skip(top).Sum(r => r.Count);
            if (rest > 0) points.Add(new ChartPoint("Diğer", rest));
            return points;
        }

        public async Task<IReadOnlyList<ChartPoint>> GetToDosByPriorityAsync(CancellationToken ct = default)
        {
            var rows = await _db.ToDos.AsNoTracking()
                .GroupBy(t => t.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return rows
                .OrderBy(r => r.Priority is null ? int.MaxValue : IndexOf(ToDoPriorities.All, r.Priority))
                .Select(r => new ChartPoint(r.Priority ?? Unspecified, r.Count))
                .ToList();
        }

        public async Task<IReadOnlyList<Activity>> GetActivitiesAsync(CancellationToken ct = default) =>
            await _db.Activities.AsNoTracking().OrderBy(a => a.ActivityTime).ToListAsync(ct);

        public async Task<StoreStatistics> GetStoreStatisticsAsync(decimal priceThreshold = 5000m, CancellationToken ct = default)
        {
            var today = _clock.GetLocalNow().Date;
            var tomorrow = today.AddDays(1);
            var products = _db.Products.AsNoTracking();
            // Ciro ve popülerlik hesaplarında iptal edilen siparişler sayılmıyor.
            var validOrders = _db.Orders.AsNoTracking().Where(o => o.SaleStatus != OrderStatuses.Cancelled);

            var mostPopular = await validOrders
                .GroupBy(o => o.Product!.ProductName)
                .Select(g => new { Name = g.Key, Quantity = g.Sum(o => o.OrderCount) })
                .OrderByDescending(x => x.Quantity)
                .FirstOrDefaultAsync(ct);

            var topCategory = await validOrders
                .GroupBy(o => o.Product!.Category!.CategoryName)
                .Select(g => new { Name = g.Key, Revenue = g.Sum(o => o.TotalPrice), Quantity = g.Sum(o => o.OrderCount) })
                .OrderByDescending(x => x.Revenue)
                .FirstOrDefaultAsync(ct);

            var highest = await products.OrderByDescending(p => p.ProductStock)
                .Select(p => new ProductStockInfo(p.ProductName, p.ProductStock)).FirstOrDefaultAsync(ct);
            var lowest = await products.OrderBy(p => p.ProductStock)
                .Select(p => new ProductStockInfo(p.ProductName, p.ProductStock)).FirstOrDefaultAsync(ct);

            return new StoreStatistics
            {
                CategoryCount = await _db.Categories.CountAsync(ct),
                ProductCount = await products.CountAsync(ct),
                MostExpensivePrice = await products.MaxAsync(p => (decimal?)p.ProductPrice, ct),
                CheapestPrice = await products.MinAsync(p => (decimal?)p.ProductPrice, ct),
                AverageProductPrice = await products.AverageAsync(p => (decimal?)p.ProductPrice, ct),
                TotalOrderCount = await _db.Orders.CountAsync(ct),
                // o.OrderDate.Date yerine aralık karşılaştırması: OrderDate üzerindeki indeks kullanılabiliyor.
                TodaysOrderCount = await _db.Orders.CountAsync(o => o.OrderDate >= today && o.OrderDate < tomorrow, ct),
                MostPopularProduct = mostPopular?.Name,
                MostPopularProductQuantity = mostPopular?.Quantity ?? 0,
                TotalStock = await products.SumAsync(p => p.ProductStock, ct),
                TotalOrderedQuantity = await validOrders.SumAsync(o => o.OrderCount, ct),
                TopRevenueCategory = topCategory?.Name,
                TopRevenueCategoryQuantity = topCategory?.Quantity ?? 0,
                HighestStock = highest,
                LowestStock = lowest,
                ProductsBelowPriceCount = await products.CountAsync(p => p.ProductPrice < priceThreshold, ct),
                PriceThreshold = priceThreshold
            };
        }

        private static int IndexOf(IReadOnlyList<string> list, string value)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == value) return i;
            }

            return list.Count;
        }
    }
}
