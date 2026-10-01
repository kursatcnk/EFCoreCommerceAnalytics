using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Models
{
    /// <summary>Bir sayfalık kayıt ve sayfalama bilgisi. ViewBag yerine view'lara tipli olarak gidiyor.</summary>
    public sealed class PagedList<T>
    {
        public PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount, string? search = null)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            Search = search;
        }

        public IReadOnlyList<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public string? Search { get; }

        public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    public static class PagingExtensions
    {
        public const int DefaultPageSize = 10;

        /// <summary>
        /// Sorguyu sayfalar. Geçersiz sayfa numaraları (0, eksi ya da son sayfadan büyük) en yakın geçerli sayfaya çekilir.
        /// </summary>
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int page, int pageSize = DefaultPageSize,
            string? search = null, CancellationToken ct = default)
        {
            var totalCount = await query.CountAsync(ct);
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return new PagedList<T>(items, page, pageSize, totalCount, search);
        }
    }
}
