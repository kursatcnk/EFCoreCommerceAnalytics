namespace EFCoreCommerceAnalytics.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        /// <summary>Bildirimin türü: "Order", "Message" gibi.</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>İlgili kaydın kimliği (ör. OrderId).</summary>
        public int ReferenceId { get; set; }

        public bool IsRead { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
    }
}
