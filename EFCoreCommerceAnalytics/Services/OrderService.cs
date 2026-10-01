using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using EFCoreCommerceAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Services
{
    public enum OrderListFilter
    {
        /// <summary>İptal edilmemiş bütün siparişler.</summary>
        Active,
        /// <summary>Yalnızca iptal edilenler.</summary>
        Cancelled
    }

    public interface IOrderService
    {
        Task<PagedList<Order>> GetPagedAsync(OrderListFilter filter, string? search, int page, bool newestFirst = false, CancellationToken ct = default);
        Task<Order?> GetAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Order>> GetLatestAsync(int take, CancellationToken ct = default);
        Task<OperationResult> CreateAsync(int customerId, int productId, int quantity, CancellationToken ct = default);
        Task<OperationResult> UpdateAsync(int id, int customerId, int productId, int quantity, string? status, CancellationToken ct = default);
        Task<OperationResult> CancelAsync(int id, CancellationToken ct = default);
    }

    public sealed class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly TimeProvider _clock;

        public OrderService(AppDbContext db, TimeProvider clock)
        {
            _db = db;
            _clock = clock;
        }

        public Task<PagedList<Order>> GetPagedAsync(OrderListFilter filter, string? search, int page, bool newestFirst = false, CancellationToken ct = default)
        {
            var query = _db.Orders.AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Product)
                .AsQueryable();

            query = filter == OrderListFilter.Cancelled
                ? query.Where(o => o.SaleStatus == OrderStatuses.Cancelled)
                : query.Where(o => o.SaleStatus != OrderStatuses.Cancelled);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o =>
                    o.Customer!.CustomerFirstName.Contains(search) ||
                    o.Customer.CustomerLastName.Contains(search) ||
                    o.Product!.ProductName.Contains(search) ||
                    (o.SaleStatus != null && o.SaleStatus.Contains(search)));
            }

            query = newestFirst ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderId);
            return query.ToPagedListAsync(page, search: search, ct: ct);
        }

        public Task<Order?> GetAsync(int id, CancellationToken ct = default) =>
            _db.Orders.AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id, ct);

        public async Task<IReadOnlyList<Order>> GetLatestAsync(int take, CancellationToken ct = default) =>
            await _db.Orders.AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(take)
                .ToListAsync(ct);

        /// <summary>
        /// Birim fiyat formdan değil, ürünün güncel fiyatından alınıyor; böylece tarayıcıda değiştirilen bir fiyat kaydedilemiyor.
        /// Yeni siparişler için navbar'da görünen bir bildirim de oluşturuluyor.
        /// </summary>
        public async Task<OperationResult> CreateAsync(int customerId, int productId, int quantity, CancellationToken ct = default)
        {
            if (quantity < 1) return OperationResult.Fail("Adet en az 1 olmalı.");

            var customer = await _db.Customers.FindAsync(new object[] { customerId }, ct);
            var product = await _db.Products.FindAsync(new object[] { productId }, ct);
            if (customer is null) return OperationResult.Fail("Seçilen müşteri bulunamadı.");
            if (product is null) return OperationResult.Fail("Seçilen ürün bulunamadı.");

            var order = new Order
            {
                CustomerId = customerId,
                ProductId = productId,
                OrderCount = quantity,
                UnitPrice = product.ProductPrice,
                TotalPrice = product.ProductPrice * quantity,
                OrderDate = _clock.GetLocalNow().DateTime,
                SaleStatus = OrderStatuses.Received
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            _db.Notifications.Add(new Notification
            {
                Type = "Order",
                ReferenceId = order.OrderId,
                Title = "Yeni sipariş",
                Content = $"{customer.FullName}: {quantity} adet {product.ProductName}",
                DateTime = order.OrderDate
            });
            await _db.SaveChangesAsync(ct);

            return OperationResult.Ok();
        }

        public async Task<OperationResult> UpdateAsync(int id, int customerId, int productId, int quantity, string? status, CancellationToken ct = default)
        {
            if (quantity < 1) return OperationResult.Fail("Adet en az 1 olmalı.");
            if (status is not null && !OrderStatuses.All.Contains(status)) return OperationResult.Fail("Geçersiz sipariş durumu.");

            var order = await _db.Orders.FindAsync(new object[] { id }, ct);
            if (order is null) return OperationResult.NotFound();

            var product = await _db.Products.FindAsync(new object[] { productId }, ct);
            if (product is null) return OperationResult.Fail("Seçilen ürün bulunamadı.");
            if (!await _db.Customers.AnyAsync(c => c.CustomerId == customerId, ct)) return OperationResult.Fail("Seçilen müşteri bulunamadı.");

            // Ürün değişmediyse siparişteki fiyat korunur; sonradan yapılan fiyat değişikliği eski siparişi değiştirmez.
            if (order.ProductId != productId)
            {
                order.UnitPrice = product.ProductPrice;
            }

            order.CustomerId = customerId;
            order.ProductId = productId;
            order.OrderCount = quantity;
            order.TotalPrice = order.UnitPrice * quantity;
            order.SaleStatus = status ?? order.SaleStatus;

            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }

        public async Task<OperationResult> CancelAsync(int id, CancellationToken ct = default)
        {
            var order = await _db.Orders.FindAsync(new object[] { id }, ct);
            if (order is null) return OperationResult.NotFound();
            if (order.SaleStatus == OrderStatuses.Delivered) return OperationResult.Fail("Teslim edilmiş bir sipariş iptal edilemez.");

            order.SaleStatus = OrderStatuses.Cancelled;
            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }
    }
}
