using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using EFCoreCommerceAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Services
{
    public interface IProductService
    {
        Task<PagedList<Product>> GetPagedAsync(string? search, int page, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Product>> SearchAsync(string? term, int take = 20, CancellationToken ct = default);
        Task<Product?> GetAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(Product product, CancellationToken ct = default);
        Task<OperationResult> UpdateAsync(int id, Product values, CancellationToken ct = default);
        Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default);
    }

    public sealed class ProductService : IProductService
    {
        private readonly AppDbContext _db;

        public ProductService(AppDbContext db) => _db = db;

        public Task<PagedList<Product>> GetPagedAsync(string? search, int page, CancellationToken ct = default)
        {
            var query = _db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || p.Category!.CategoryName.Contains(search));
            }

            return query.OrderBy(p => p.ProductId).ToPagedListAsync(page, search: search, ct: ct);
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
            await _db.Products.AsNoTracking().OrderBy(p => p.ProductName).ToListAsync(ct);

        public async Task<IReadOnlyList<Product>> SearchAsync(string? term, int take = 20, CancellationToken ct = default)
        {
            var query = _db.Products.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(p => p.ProductName.Contains(term));
            }

            return await query.OrderBy(p => p.ProductName).Take(take).ToListAsync(ct);
        }

        public Task<Product?> GetAsync(int id, CancellationToken ct = default) =>
            _db.Products.AsNoTracking().Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id, ct);

        public async Task<int> CreateAsync(Product product, CancellationToken ct = default)
        {
            var entity = new Product
            {
                ProductName = product.ProductName.Trim(),
                ProductPrice = product.ProductPrice,
                ProductStock = product.ProductStock,
                CategoryId = product.CategoryId
            };
            _db.Products.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.ProductId;
        }

        public async Task<OperationResult> UpdateAsync(int id, Product values, CancellationToken ct = default)
        {
            // Formdan gelen nesne doğrudan Update edilmiyor; yalnızca düzenlenebilir alanlar kopyalanıyor.
            var product = await _db.Products.FindAsync(new object[] { id }, ct);
            if (product is null) return OperationResult.NotFound();

            product.ProductName = values.ProductName.Trim();
            product.ProductPrice = values.ProductPrice;
            product.ProductStock = values.ProductStock;
            product.CategoryId = values.CategoryId;
            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }

        public async Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            var product = await _db.Products.FindAsync(new object[] { id }, ct);
            if (product is null) return OperationResult.NotFound();

            if (await _db.Orders.AnyAsync(o => o.ProductId == id, ct))
            {
                return OperationResult.Fail("Bu ürünün siparişleri var; silinirse satış geçmişi bozulur. Stoğunu sıfırlamayı düşünebilirsiniz.");
            }

            _db.Products.Remove(product);
            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }
    }
}
