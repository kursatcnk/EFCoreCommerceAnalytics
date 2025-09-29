namespace EFCoreCommerceAnalytics.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerCity { get; set; }
        public string? CustomerDistrict { get; set; }
        public decimal CustomerBalance { get; set; }
        public string? CustomerImageUrl { get; set; }
        public List<Order> Orders { get; set; }
    }
}
