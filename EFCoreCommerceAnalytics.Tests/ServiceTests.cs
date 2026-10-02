using EFCoreCommerceAnalytics.Entities;
using EFCoreCommerceAnalytics.Models;
using EFCoreCommerceAnalytics.Services;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Tests
{
    public class PagingTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(-3, 1)]
        [InlineData(2, 2)]
        [InlineData(99, 3)]
        public async Task Page_number_is_clamped_to_existing_pages(int requested, int expected)
        {
            using var db = TestDb.Create();
            db.Categories.AddRange(Enumerable.Range(1, 25).Select(i => new Category { CategoryName = $"K{i}", Status = true }));
            await db.SaveChangesAsync();

            var page = await db.Categories.OrderBy(c => c.CategoryId).ToPagedListAsync(requested, pageSize: 10);

            Assert.Equal(expected, page.Page);
            Assert.Equal(3, page.TotalPages);
            Assert.Equal(25, page.TotalCount);
        }

        [Fact]
        public async Task Empty_table_has_one_empty_page()
        {
            using var db = TestDb.Create();
            var page = await db.Categories.OrderBy(c => c.CategoryId).ToPagedListAsync(5);

            Assert.Equal(1, page.Page);
            Assert.Equal(1, page.TotalPages);
            Assert.Empty(page.Items);
        }
    }

    public class CategoryServiceTests
    {
        [Fact]
        public async Task Category_with_products_is_not_deleted()
        {
            using var db = TestDb.Create();
            var (category, _, _) = TestDb.AddBasics(db);

            var result = await new CategoryService(db).DeleteAsync(category.CategoryId);

            Assert.False(result.Succeeded);
            Assert.True(await db.Categories.AnyAsync());
        }

        [Fact]
        public async Task Empty_category_is_deleted()
        {
            using var db = TestDb.Create();
            var service = new CategoryService(db);
            var id = await service.CreateAsync("  Boş  ");

            Assert.Equal("Boş", (await service.GetAsync(id))!.CategoryName);
            Assert.True((await service.DeleteAsync(id)).Succeeded);
            Assert.False(await db.Categories.AnyAsync());
        }

        [Fact]
        public async Task Missing_category_reports_not_found()
        {
            using var db = TestDb.Create();
            var result = await new CategoryService(db).UpdateAsync(42, "x", true);
            Assert.True(result.IsNotFound);
        }
    }

    public class OrderServiceTests
    {
        private static OrderService Service(EFCoreCommerceAnalytics.Context.AppDbContext db) =>
            new(db, new FixedClock(new DateTime(2026, 10, 2, 9, 30, 0)));

        [Fact]
        public async Task New_order_takes_price_from_product_and_creates_notification()
        {
            using var db = TestDb.Create();
            var (_, product, customer) = TestDb.AddBasics(db, price: 249.90m);

            var result = await Service(db).CreateAsync(customer.CustomerId, product.ProductId, 3);

            Assert.True(result.Succeeded);
            var order = await db.Orders.SingleAsync();
            Assert.Equal(249.90m, order.UnitPrice);
            Assert.Equal(749.70m, order.TotalPrice);
            Assert.Equal(OrderStatuses.Received, order.SaleStatus);
            Assert.Equal(new DateTime(2026, 10, 2, 9, 30, 0), order.OrderDate);

            var notification = await db.Notifications.SingleAsync();
            Assert.Equal(order.OrderId, notification.ReferenceId);
            Assert.False(notification.IsRead);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Quantity_must_be_positive(int quantity)
        {
            using var db = TestDb.Create();
            var (_, product, customer) = TestDb.AddBasics(db);

            var result = await Service(db).CreateAsync(customer.CustomerId, product.ProductId, quantity);

            Assert.False(result.Succeeded);
            Assert.False(await db.Orders.AnyAsync());
        }

        [Fact]
        public async Task Unknown_product_is_rejected()
        {
            using var db = TestDb.Create();
            var (_, _, customer) = TestDb.AddBasics(db);

            var result = await Service(db).CreateAsync(customer.CustomerId, 999, 1);

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task Update_keeps_original_price_when_product_is_unchanged()
        {
            using var db = TestDb.Create();
            var (_, product, customer) = TestDb.AddBasics(db, price: 100m);
            var order = TestDb.AddOrder(db, customer, product);
            product.ProductPrice = 150m;
            await db.SaveChangesAsync();

            var result = await Service(db).UpdateAsync(order.OrderId, customer.CustomerId, product.ProductId, 2, OrderStatuses.InTransit);

            Assert.True(result.Succeeded);
            Assert.Equal(100m, order.UnitPrice);
            Assert.Equal(200m, order.TotalPrice);
            Assert.Equal(OrderStatuses.InTransit, order.SaleStatus);
        }

        [Fact]
        public async Task Unknown_status_is_rejected()
        {
            using var db = TestDb.Create();
            var (_, product, customer) = TestDb.AddBasics(db);
            var order = TestDb.AddOrder(db, customer, product);

            var result = await Service(db).UpdateAsync(order.OrderId, customer.CustomerId, product.ProductId, 1, "Uydurma");

            Assert.False(result.Succeeded);
            Assert.Equal(OrderStatuses.Received, order.SaleStatus);
        }

        [Fact]
        public async Task Delivered_order_cannot_be_cancelled()
        {
            using var db = TestDb.Create();
            var (_, product, customer) = TestDb.AddBasics(db);
            var order = TestDb.AddOrder(db, customer, product, status: OrderStatuses.Delivered);

            Assert.False((await Service(db).CancelAsync(order.OrderId)).Succeeded);
            Assert.Equal(OrderStatuses.Delivered, order.SaleStatus);
        }

        [Fact]
        public async Task Cancelled_orders_are_listed_separately()
        {
            using var db = TestDb.Create();
            var (_, product, customer) = TestDb.AddBasics(db);
            TestDb.AddOrder(db, customer, product);
            var cancelled = TestDb.AddOrder(db, customer, product);
            await Service(db).CancelAsync(cancelled.OrderId);

            var active = await Service(db).GetPagedAsync(OrderListFilter.Active, null, 1);
            var cancelledList = await Service(db).GetPagedAsync(OrderListFilter.Cancelled, null, 1);

            Assert.Equal(1, active.TotalCount);
            Assert.Equal(cancelled.OrderId, Assert.Single(cancelledList.Items).OrderId);
        }
    }

    public class CustomerServiceTests
    {
        [Fact]
        public async Task Customer_with_orders_is_not_deleted()
        {
            using var db = TestDb.Create();
            var (_, product, customer) = TestDb.AddBasics(db);
            TestDb.AddOrder(db, customer, product);

            Assert.False((await new CustomerService(db).DeleteAsync(customer.CustomerId)).Succeeded);
        }

        [Fact]
        public async Task High_balance_filter_runs_in_query_and_sorts_by_balance()
        {
            using var db = TestDb.Create();
            db.Customers.AddRange(
                new Customer { CustomerFirstName = "A", CustomerLastName = "1", CustomerCity = "X", CustomerBalance = 999.99m },
                new Customer { CustomerFirstName = "B", CustomerLastName = "2", CustomerCity = "X", CustomerBalance = 1000m },
                new Customer { CustomerFirstName = "C", CustomerLastName = "3", CustomerCity = "X", CustomerBalance = 5000m });
            await db.SaveChangesAsync();

            var page = await new CustomerService(db).GetWithBalanceAtLeastAsync(1000m, 1);

            Assert.Equal(new[] { "C", "B" }, page.Items.Select(c => c.CustomerFirstName));
        }

        [Fact]
        public async Task Top_cities_pick_the_best_customer_and_ignore_cancelled_orders()
        {
            using var db = TestDb.Create();
            var (_, product, ayse) = TestDb.AddBasics(db);
            var mehmet = new Customer { CustomerFirstName = "Mehmet", CustomerLastName = "Kaya", CustomerCity = "İstanbul" };
            var zeynep = new Customer { CustomerFirstName = "Zeynep", CustomerLastName = "Demir", CustomerCity = "Ankara" };
            db.AddRange(mehmet, zeynep);
            await db.SaveChangesAsync();

            TestDb.AddOrder(db, ayse, product);
            TestDb.AddOrder(db, mehmet, product);
            TestDb.AddOrder(db, mehmet, product);
            TestDb.AddOrder(db, zeynep, product);
            // İptal edilenler sayılmamalı: aksi hâlde Ankara İstanbul'u geçerdi.
            for (var i = 0; i < 5; i++) TestDb.AddOrder(db, zeynep, product, status: OrderStatuses.Cancelled);

            var rows = await new CustomerService(db).GetTopCitiesWithTopCustomersAsync(2);

            Assert.Equal("İstanbul", rows[0].City);
            Assert.Equal(3, rows[0].TotalOrders);
            Assert.Equal("Mehmet Kaya", rows[0].CustomerName);
            Assert.Equal(2, rows[0].CustomerOrderCount);
            Assert.Equal("Ankara", rows[1].City);
            Assert.Equal(1, rows[1].TotalOrders);
        }
    }

    public class ReportServiceTests
    {
        private static ReportService Service(EFCoreCommerceAnalytics.Context.AppDbContext db) =>
            new(db, new FixedClock(new DateTime(2026, 10, 2, 12, 0, 0)));

        [Fact]
        public async Task Reports_do_not_throw_on_an_empty_database()
        {
            using var db = TestDb.Create();
            var service = Service(db);

            var summary = await service.GetSummaryAsync();
            var stats = await service.GetStoreStatisticsAsync();

            Assert.Null(summary.AverageCustomerBalance);
            Assert.Null(stats.MostExpensivePrice);
            Assert.Null(stats.MostPopularProduct);
            Assert.Null(stats.HighestStock);
        }

        [Fact]
        public async Task Daily_counts_include_days_without_orders()
        {
            using var db = TestDb.Create();
            var (_, product, customer) = TestDb.AddBasics(db);
            TestDb.AddOrder(db, customer, product, date: new DateTime(2026, 10, 2, 9, 0, 0));
            TestDb.AddOrder(db, customer, product, date: new DateTime(2026, 10, 2, 18, 0, 0));
            TestDb.AddOrder(db, customer, product, date: new DateTime(2026, 9, 30, 10, 0, 0));
            TestDb.AddOrder(db, customer, product, date: new DateTime(2026, 9, 1, 10, 0, 0)); // aralığın dışında

            var points = await Service(db).GetDailyOrderCountsAsync(7);

            Assert.Equal(7, points.Count);
            Assert.Equal(new[] { 0, 0, 0, 0, 1, 0, 2 }, points.Select(p => p.Value));
            Assert.Equal("02.10", points[^1].Label);
        }

        [Fact]
        public async Task Revenue_and_popularity_ignore_cancelled_orders()
        {
            using var db = TestDb.Create();
            var (category, cheap, customer) = TestDb.AddBasics(db, price: 10m);
            var expensive = new Product { ProductName = "Pahalı", ProductPrice = 1000m, ProductStock = 1, Category = new Category { CategoryName = "Lüks", Status = true } };
            db.Products.Add(expensive);
            await db.SaveChangesAsync();

            TestDb.AddOrder(db, customer, cheap, count: 3);
            TestDb.AddOrder(db, customer, expensive, count: 5, status: OrderStatuses.Cancelled);

            var stats = await Service(db).GetStoreStatisticsAsync();

            Assert.Equal("Kulaklık", stats.MostPopularProduct);
            Assert.Equal(category.CategoryName, stats.TopRevenueCategory);
            Assert.Equal(3, stats.TotalOrderedQuantity);
            Assert.Equal(2, stats.TotalOrderCount);
        }

        [Fact]
        public async Task Small_cities_are_grouped_as_other()
        {
            using var db = TestDb.Create();
            foreach (var (city, count) in new[] { ("İstanbul", 5), ("Ankara", 3), ("İzmir", 2), ("Bursa", 1) })
            {
                for (var i = 0; i < count; i++) db.Customers.Add(new Customer { CustomerFirstName = "x", CustomerLastName = "y", CustomerCity = city });
            }
            await db.SaveChangesAsync();

            var points = await Service(db).GetCustomersByCityAsync(top: 2);

            Assert.Equal(new[] { "İstanbul", "Ankara", "Diğer" }, points.Select(p => p.Label));
            Assert.Equal(3, points[^1].Value);
        }
    }

    public class ToDoServiceTests
    {
        [Fact]
        public async Task Linq_examples_use_the_real_priority_values()
        {
            using var db = TestDb.Create();
            var service = new ToDoService(db);
            foreach (var priority in ToDoPriorities.All)
            {
                await service.CreateAsync($"{priority} görev", false, priority);
            }

            Assert.Equal(2, (await service.GetFirstAndSecondConcatAsync()).Count);
            Assert.Equal(2, (await service.GetFourthAndFifthUnionAsync()).Count);

            var (items, joined) = await service.GetFirstPriorityWithAggregateAsync();
            Assert.Single(items);
            Assert.Equal("Birincil görev", joined);
        }

        [Fact]
        public async Task Chunks_split_into_groups_of_three()
        {
            using var db = TestDb.Create();
            var service = new ToDoService(db);
            for (var i = 0; i < 7; i++) await service.CreateAsync($"G{i}", false, ToDoPriorities.First);

            var chunks = await service.GetFirstPriorityInChunksAsync(3);

            Assert.Equal(new[] { 3, 3, 1 }, chunks.Select(c => c.Length));
        }
    }
}
