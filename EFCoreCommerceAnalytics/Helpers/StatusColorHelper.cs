using EFCoreCommerceAnalytics.Entities;

namespace EFCoreCommerceAnalytics.Helpers
{
    public static class StatusColorHelper
    {
        public static string GetBadgeClass(string? status)
        {
            return status switch
            {
                OrderStatuses.Delivered => "badge badge-success badge-pill",
                OrderStatuses.InTransit => "badge badge-primary badge-pill",
                OrderStatuses.OutForDelivery => "badge badge-warning badge-pill",
                OrderStatuses.Received => "badge badge-info badge-pill",
                // Eski kayıtlarda kısa "İptal" yazımı da bulunabiliyor.
                OrderStatuses.Cancelled or "İptal" => "badge badge-danger badge-pill",
                _ => "badge badge-secondary badge-pill"
            };
        }
    }
}
