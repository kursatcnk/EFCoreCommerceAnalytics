using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using EFCoreCommerceAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Services
{
    public interface ICategoryService
    {
        Task<PagedList<Category>> GetPagedAsync(string? search, int page, CancellationToken ct = default);
        Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
        Task<Category?> GetAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(string name, CancellationToken ct = default);
        Task<OperationResult> UpdateAsync(int id, string name, bool status, CancellationToken ct = default);
        Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default);
    }

    public sealed class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db) => _db = db;

        public Task<PagedList<Category>> GetPagedAsync(string? search, int page, CancellationToken ct = default)
        {
            var query = _db.Categories.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.CategoryName.Contains(search));
            }

            return query.OrderBy(c => c.CategoryId).ToPagedListAsync(page, search: search, ct: ct);
        }

        public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
            await _db.Categories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync(ct);

        public Task<Category?> GetAsync(int id, CancellationToken ct = default) =>
            _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id, ct);

        public async Task<int> CreateAsync(string name, CancellationToken ct = default)
        {
            var category = new Category { CategoryName = name.Trim(), Status = true };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync(ct);
            return category.CategoryId;
        }

        public async Task<OperationResult> UpdateAsync(int id, string name, bool status, CancellationToken ct = default)
        {
            var category = await _db.Categories.FindAsync(new object[] { id }, ct);
            if (category is null) return OperationResult.NotFound();

            category.CategoryName = name.Trim();
            category.Status = status;
            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }

        public async Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            var category = await _db.Categories.FindAsync(new object[] { id }, ct);
            if (category is null) return OperationResult.NotFound();

            if (await _db.Products.AnyAsync(p => p.CategoryId == id, ct))
            {
                return OperationResult.Fail("Bu kategoride ürün var. Önce ürünleri başka bir kategoriye taşıyın ya da kategoriyi pasif yapın.");
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }
    }
}
