using EFCoreCommerceAnalytics.Entities;
using EFCoreCommerceAnalytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents
{
    public sealed record NavbarModel(
        int UnreadNotificationCount,
        IReadOnlyList<Notification> Notifications,
        int UnreadMessageCount,
        IReadOnlyList<Message> Messages);

    /// <summary>
    /// Üst menü. Şablondaki sabit "16 bildirim", "25 mesaj" ve uydurma isimler yerine
    /// veritabanındaki bildirimler ve okunmamış mesajlar gösteriliyor.
    /// </summary>
    public sealed class NavbarViewComponent : ViewComponent
    {
        private readonly IInboxService _inbox;

        public NavbarViewComponent(IInboxService inbox) => _inbox = inbox;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var ct = HttpContext.RequestAborted;
            return View(new NavbarModel(
                await _inbox.CountUnreadNotificationsAsync(ct),
                await _inbox.GetLatestNotificationsAsync(5, ct),
                await _inbox.CountUnreadMessagesAsync(ct),
                await _inbox.GetLatestMessagesAsync(3, ct)));
        }
    }
}
