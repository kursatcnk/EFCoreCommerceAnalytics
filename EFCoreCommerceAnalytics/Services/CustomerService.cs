using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using EFCoreCommerceAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Services
{
    public interface ICustomerService
    {
        Task<PagedList<Customer>> GetPagedAsync(string? search, int page, CancellationToken ct = default);
        Task<PagedList<Customer>> GetWithBalanceAtLeastAsync(decimal threshold, int page, CancellationToken ct = default);
        Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Customer>> SearchAsync(string? term, int take = 20, CancellationToken ct = default);
        Task<Customer?> GetAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(Customer customer, CancellationToken ct = default);
        Task<OperationResult> UpdateAsync(int id, Customer values, CancellationToken ct = default);
        Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<CityCustomerCount>> GetCustomerCountsByCityAsync(CancellationToken ct = default);
        Task<IReadOnlyList<TopCityCustomer>> GetTopCitiesWithTopCustomersAsync(int cityCount = 3, CancellationToken ct = default);
    }

    public sealed class CustomerService : ICustomerService
    {
        private readonly AppDbContext _db;

        public CustomerService(AppDbContext db) => _db = db;

        public Task<PagedList<Customer>> GetPagedAsync(string? search, int page, CancellationToken ct = default)
        {
            var query = _db.Customers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.CustomerFirstName.Contains(search) ||
                    c.CustomerLastName.Contains(search) ||
                    c.CustomerCity.Contains(search));
            }

            return query.OrderBy(c => c.CustomerId).ToPagedListAsync(page, search: search, ct: ct);
        }

        /// <summary>
        /// Önceden bütün müşteriler belleğe çekilip Except ile süzülüyordu; filtre artık veritabanında çalışıyor.
        /// </summary>
        public Task<PagedList<Customer>> GetWithBalanceAtLeastAsync(decimal threshold, int page, CancellationToken ct = default) =>
            _db.Customers.AsNoTracking()
                .Where(c => c.CustomerBalance >= threshold)
                .OrderByDescending(c => c.CustomerBalance)
                .ToPagedListAsync(page, ct: ct);

        public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default) =>
            await _db.Customers.AsNoTracking().OrderBy(c => c.CustomerFirstName).ThenBy(c => c.CustomerLastName).ToListAsync(ct);

        public async Task<IReadOnlyList<Customer>> SearchAsync(string? term, int take = 20, CancellationToken ct = default)
        {
            var query = _db.Customers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(c => c.CustomerFirstName.Contains(term) || c.CustomerLastName.Contains(term));
            }

            return await query.OrderBy(c => c.CustomerFirstName).Take(take).ToListAsync(ct);
        }

        public Task<Customer?> GetAsync(int id, CancellationToken ct = default) =>
            _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == id, ct);

        public async Task<int> CreateAsync(Customer customer, CancellationToken ct = default)
        {
            var entity = new Customer();
            Copy(customer, entity);
            _db.Customers.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.CustomerId;
        }

        public async Task<OperationResult> UpdateAsync(int id, Customer values, CancellationToken ct = default)
        {
            var customer = await _db.Customers.FindAsync(new object[] { id }, ct);
            if (customer is null) return OperationResult.NotFound();

            Copy(values, customer);
            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }

        public async Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            var customer = await _db.Customers.FindAsync(new object[] { id }, ct);
            if (customer is null) return OperationResult.NotFound();

            if (await _db.Orders.AnyAsync(o => o.CustomerId == id, ct))
            {
                return OperationResult.Fail("Bu müşterinin siparişleri var; silinirse satış raporları eksik kalır.");
            }

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }

        public async Task<IReadOnlyList<CityCustomerCount>> GetCustomerCountsByCityAsync(CancellationToken ct = default)
        {
            // Sıralama SQL'de yapılabilsin diye önce anonim tipe, sonra bellekte record'a çevriliyor.
            var rows = await _db.Customers.AsNoTracking()
                .GroupBy(c => c.CustomerCity)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(ct);

            return rows.Select(r => new CityCustomerCount(r.City, r.Count)).ToList();
        }

        /// <summary>
        /// En çok sipariş alan şehirler ve her şehirde en çok sipariş veren müşteri.
        /// Eskiden her şehir için ayrı sorgu atılıyordu (N+1); artık iki sorguyla hesaplanıyor.
        /// İptal edilen siparişler sayılmıyor.
        /// </summary>
        public async Task<IReadOnlyList<TopCityCustomer>> GetTopCitiesWithTopCustomersAsync(int cityCount = 3, CancellationToken ct = default)
        {
            var validOrders = _db.Orders.AsNoTracking().Where(o => o.SaleStatus != OrderStatuses.Cancelled || o.SaleStatus == null);

            var topCities = await validOrders
                .GroupBy(o => o.Customer!.CustomerCity)
                .Select(g => new { City = g.Key, TotalOrders = g.Count() })
                .OrderByDescending(x => x.TotalOrders)
                .Take(cityCount)
                .ToListAsync(ct);

            var cityNames = topCities.Select(c => c.City).ToList();
            var customerCounts = await validOrders
                .Where(o => cityNames.Contains(o.Customer!.CustomerCity))
                .GroupBy(o => new { o.Customer!.CustomerCity, o.CustomerId, o.Customer.CustomerFirstName, o.Customer.CustomerLastName })
                .Select(g => new
                {
                    City = g.Key.CustomerCity,
                    g.Key.CustomerId,
                    Name = g.Key.CustomerFirstName + " " + g.Key.CustomerLastName,
                    Count = g.Count()
                })
                .ToListAsync(ct);

            return topCities
                .Select(city =>
                {
                    var best = customerCounts.Where(c => c.City == city.City).MaxBy(c => c.Count);
                    return new TopCityCustomer(city.City, city.TotalOrders, best?.CustomerId ?? 0, best?.Name ?? "-", best?.Count ?? 0);
                })
                .ToList();
        }

        private static void Copy(Customer source, Customer target)
        {
            target.CustomerFirstName = source.CustomerFirstName.Trim();
            target.CustomerLastName = source.CustomerLastName.Trim();
            target.CustomerCity = source.CustomerCity.Trim();
            target.CustomerDistrict = string.IsNullOrWhiteSpace(source.CustomerDistrict) ? null : source.CustomerDistrict.Trim();
            target.CustomerBalance = source.CustomerBalance;
            target.CustomerImageUrl = string.IsNullOrWhiteSpace(source.CustomerImageUrl) ? null : source.CustomerImageUrl.Trim();
        }
    }
}
