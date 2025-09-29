namespace EFCoreCommerceAnalytics.Entities
{
    public class Message
    {
        public int MessageId { get; set; }
        public string Title { get; set; }

        public string SenderNameSurname { get; set; }

        public string Content { get; set; }

        public DateTime DateTime { get; set; }
        public bool IsRead { get; set; }

        public string? SenderImageUrl { get; set; }

    }
}
