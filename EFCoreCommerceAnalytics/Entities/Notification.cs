namespace EFCoreCommerceAnalytics.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        // Bildirimin türü: "Order", "Message", vs.
        public string Type { get; set; }

        // İlgili kayıt ID'si (örn. OrderId)
        public int ReferenceId { get; set; }

        // Bildirimi okunup okunmadığı
        public bool IsRead { get; set; } = false;

        // Başlık ve açıklama
        public string Title { get; set; }
        public string Content { get; set; }

        // Tarih bilgisi
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
    }

}
