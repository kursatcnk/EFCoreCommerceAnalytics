namespace EFCoreCommerceAnalytics.Entities
{
    public class Activity
    {
        public int ActivityId { get; set; }
        public string ActivityTitle { get; set; } = string.Empty;
        public string ActivityDescription { get; set; } = string.Empty;
        public TimeOnly ActivityTime { get; set; }
    }
}
