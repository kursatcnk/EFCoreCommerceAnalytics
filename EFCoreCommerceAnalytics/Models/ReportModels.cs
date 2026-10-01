namespace EFCoreCommerceAnalytics.Models
{
    /// <summary>Grafiklerde kullanılan etiket/değer çifti.</summary>
    public sealed record ChartPoint(string Label, int Value);

    /// <summary>_ChartCard partial'ına giden veri: başlık, Font Awesome ikonu, Chart.js tipi ve noktalar.</summary>
    public sealed record ChartCardModel(string Title, string Icon, string Type, IReadOnlyList<ChartPoint> Points);

    public sealed record CityCustomerCount(string City, int CustomerCount);

    public sealed record TopCityCustomer(string City, int TotalOrders, int CustomerId, string CustomerName, int CustomerOrderCount);

    public sealed record DashboardSummary(
        int CustomerCount,
        int CategoryCount,
        int ProductCount,
        decimal? AverageCustomerBalance,
        int OrderCount,
        int OrderedItemCount);

    public sealed record ProductStockInfo(string ProductName, int Stock);

    /// <summary>İstatistik sayfasındaki kartların tamamı. Boş tablolarda değerler null/0 olur, sayfa çökmez.</summary>
    public sealed class StoreStatistics
    {
        public int CategoryCount { get; init; }
        public int ProductCount { get; init; }
        public decimal? MostExpensivePrice { get; init; }
        public decimal? CheapestPrice { get; init; }
        public decimal? AverageProductPrice { get; init; }
        public int TotalOrderCount { get; init; }
        public int TodaysOrderCount { get; init; }
        public string? MostPopularProduct { get; init; }
        public int MostPopularProductQuantity { get; init; }
        public int TotalStock { get; init; }
        public int TotalOrderedQuantity { get; init; }
        public string? TopRevenueCategory { get; init; }
        public int TopRevenueCategoryQuantity { get; init; }
        public ProductStockInfo? HighestStock { get; init; }
        public ProductStockInfo? LowestStock { get; init; }
        public int ProductsBelowPriceCount { get; init; }
        public decimal PriceThreshold { get; init; }
    }
}
