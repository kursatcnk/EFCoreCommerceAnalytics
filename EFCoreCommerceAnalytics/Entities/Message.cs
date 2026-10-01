namespace EFCoreCommerceAnalytics.Entities
{
    public class Message
    {
        public int MessageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SenderNameSurname { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public bool IsRead { get; set; }
        public string? SenderImageUrl { get; set; }
    }
}
