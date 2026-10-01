using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using EFCoreCommerceAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Services
{
    /// <summary>Mesajlar ve bildirimler: mesaj listesi, mesaj detayı ve navbar'daki sayaçlar.</summary>
    public interface IInboxService
    {
        Task<PagedList<Message>> GetMessagesAsync(int page, CancellationToken ct = default);
        Task<IReadOnlyList<Message>> GetLatestMessagesAsync(int take, CancellationToken ct = default);
        Task<Message?> OpenMessageAsync(int id, CancellationToken ct = default);
        Task<int> CountUnreadMessagesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Notification>> GetLatestNotificationsAsync(int take, CancellationToken ct = default);
        Task<int> CountUnreadNotificationsAsync(CancellationToken ct = default);
        Task MarkNotificationsReadAsync(CancellationToken ct = default);
    }

    public sealed class InboxService : IInboxService
    {
        private readonly AppDbContext _db;

        public InboxService(AppDbContext db) => _db = db;

        public Task<PagedList<Message>> GetMessagesAsync(int page, CancellationToken ct = default) =>
            _db.Messages.AsNoTracking().OrderByDescending(m => m.DateTime).ToPagedListAsync(page, ct: ct);

        public async Task<IReadOnlyList<Message>> GetLatestMessagesAsync(int take, CancellationToken ct = default) =>
            await _db.Messages.AsNoTracking().OrderByDescending(m => m.DateTime).Take(take).ToListAsync(ct);

        /// <summary>Mesajı getirir ve okunmamışsa okundu olarak işaretler.</summary>
        public async Task<Message?> OpenMessageAsync(int id, CancellationToken ct = default)
        {
            var message = await _db.Messages.FindAsync(new object[] { id }, ct);
            if (message is { IsRead: false })
            {
                message.IsRead = true;
                await _db.SaveChangesAsync(ct);
            }

            return message;
        }

        public Task<int> CountUnreadMessagesAsync(CancellationToken ct = default) =>
            _db.Messages.CountAsync(m => !m.IsRead, ct);

        public async Task<IReadOnlyList<Notification>> GetLatestNotificationsAsync(int take, CancellationToken ct = default) =>
            await _db.Notifications.AsNoTracking().OrderByDescending(n => n.DateTime).Take(take).ToListAsync(ct);

        public Task<int> CountUnreadNotificationsAsync(CancellationToken ct = default) =>
            _db.Notifications.CountAsync(n => !n.IsRead, ct);

        public Task MarkNotificationsReadAsync(CancellationToken ct = default) =>
            _db.Notifications.Where(n => !n.IsRead).ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }
}
