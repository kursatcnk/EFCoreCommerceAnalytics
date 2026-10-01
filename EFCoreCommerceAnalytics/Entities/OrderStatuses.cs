namespace EFCoreCommerceAnalytics.Entities
{
    /// <summary>
    /// Sipariş durumları veritabanında metin olarak tutuluyor. Değerleri tek yerde toplamak,
    /// "İptal" / "İptal Edildi" gibi yazım farklarının sorguları sessizce boşa düşürmesini önlüyor.
    /// </summary>
    public static class OrderStatuses
    {
        public const string Received = "Sipariş Alındı";
        public const string InTransit = "Taşımada";
        public const string OutForDelivery = "Dağıtımda";
        public const string Delivered = "Teslim Edildi";
        public const string Cancelled = "İptal Edildi";

        /// <summary>Formlarda seçilebilen durumlar, akış sırasıyla.</summary>
        public static readonly IReadOnlyList<string> All = new[] { Received, InTransit, OutForDelivery, Delivered, Cancelled };
    }
}
