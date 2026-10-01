namespace EFCoreCommerceAnalytics.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerFirstName { get; set; } = string.Empty;
        public string CustomerLastName { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string? CustomerDistrict { get; set; }
        public decimal CustomerBalance { get; set; }
        public string? CustomerImageUrl { get; set; }

        public List<Order> Orders { get; set; } = new();

        public string FullName => $"{CustomerFirstName} {CustomerLastName}";
    }
}
