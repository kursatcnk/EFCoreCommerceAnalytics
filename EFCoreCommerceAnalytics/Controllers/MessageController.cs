using EFCoreCommerceAnalytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class MessageController : AppController
    {
        private readonly IInboxService _inbox;

        public MessageController(IInboxService inbox) => _inbox = inbox;

        public async Task<IActionResult> MessageList(int page = 1, CancellationToken ct = default) =>
            View(await _inbox.GetMessagesAsync(page, ct));

        /// <summary>Listedeki "Mesaj Detayı" butonu var olmayan bir sayfaya gidiyordu. Mesajı açınca okundu sayılıyor.</summary>
        public async Task<IActionResult> Detail(int id, CancellationToken ct)
        {
            var message = await _inbox.OpenMessageAsync(id, ct);
            return message is null ? NotFound() : View(message);
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationsRead(string? returnUrl, CancellationToken ct)
        {
            await _inbox.MarkNotificationsReadAsync(ct);
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Dashboard");
        }
    }
}
