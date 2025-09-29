namespace EFCoreCommerceAnalytics.Helpers
{
    public static class StatusColorHelper
    {
        public static string GetBadgeClass(string status)
        {
            return status switch
            {
                "Teslim Edildi" => "badge badge-success badge-pill",  // yeşil
                "Taşımada" => "badge badge-primary badge-pill",  // mavi
                "Dağıtımda" => "badge badge-warning badge-pill",  // turuncu
                "Sipariş Alındı" => "badge badge-info badge-pill",     // açık mavi
                "İptal" => "badge badge-danger badge-pill",   // kırmızı
                _ => "badge badge-secondary badge-pill" // varsayılan gri
            };
        }
    }
}
