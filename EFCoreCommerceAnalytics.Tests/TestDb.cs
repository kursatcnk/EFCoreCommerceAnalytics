using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Tests
{
    /// <summary>Her test için ayrı, boş bir InMemory veritabanı ve küçük örnek veri yardımcıları.</summary>
    internal static class TestDb
    {
        public static AppDbContext Create() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        public static (Category Category, Product Product, Customer Customer) AddBasics(AppDbContext db, decimal price = 100m)
        {
            var category = new Category { CategoryName = "Elektronik", Status = true };
            var product = new Product { ProductName = "Kulaklık", ProductPrice = price, ProductStock = 10, Category = category };
            var customer = new Customer { CustomerFirstName = "Ayşe", CustomerLastName = "Yılmaz", CustomerCity = "İstanbul", CustomerBalance = 500 };
            db.AddRange(category, product, customer);
            db.SaveChanges();
            return (category, product, customer);
        }

        public static Order AddOrder(AppDbContext db, Customer customer, Product product, int count = 1, string status = OrderStatuses.Received, DateTime? date = null)
        {
            var order = new Order
            {
                Customer = customer,
                Product = product,
                OrderCount = count,
                UnitPrice = product.ProductPrice,
                TotalPrice = product.ProductPrice * count,
                OrderDate = date ?? new DateTime(2026, 10, 1, 12, 0, 0),
                SaleStatus = status
            };
            db.Orders.Add(order);
            db.SaveChanges();
            return order;
        }
    }

    /// <summary>Testlerde "şimdi"yi sabitlemek için.</summary>
    internal sealed class FixedClock : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FixedClock(DateTime localNow) => _now = new DateTimeOffset(localNow, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
