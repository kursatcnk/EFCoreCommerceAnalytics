namespace EFCoreCommerceAnalytics.Entities
{
    public class Activity
    {
        public int ActivityId { get; set; }
        public string ActivityTitle { get; set; }
        public string ActivityDescription { get; set; }
        public TimeOnly ActivityTime { get; set; }
    }
}
