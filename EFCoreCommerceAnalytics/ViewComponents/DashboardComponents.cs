using EFCoreCommerceAnalytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.ViewComponents
{
    // Dashboard ve istatistik sayfasının parçaları. Hepsi sorgularını servislerden alıyor; view'lara tipli model gidiyor.

    /// <summary>Üstteki sayaç şeridi: müşteri, kategori, ürün, ortalama bakiye, sipariş.</summary>
    public sealed class DashboardSummaryViewComponent : ViewComponent
    {
        private readonly IReportService _reports;
        public DashboardSummaryViewComponent(IReportService reports) => _reports = reports;

        public async Task<IViewComponentResult> InvokeAsync() => View(await _reports.GetSummaryAsync(HttpContext.RequestAborted));
    }

    public sealed class OrderStatusChartViewComponent : ViewComponent
    {
        private readonly IReportService _reports;
        public OrderStatusChartViewComponent(IReportService reports) => _reports = reports;

        public async Task<IViewComponentResult> InvokeAsync() => View(await _reports.GetOrdersByStatusAsync(HttpContext.RequestAborted));
    }

    public sealed class DailyOrdersChartViewComponent : ViewComponent
    {
        private readonly IReportService _reports;
        public DailyOrdersChartViewComponent(IReportService reports) => _reports = reports;

        public async Task<IViewComponentResult> InvokeAsync(int days = 30) => View(await _reports.GetDailyOrderCountsAsync(days, HttpContext.RequestAborted));
    }

    public sealed class CustomerCityChartViewComponent : ViewComponent
    {
        private readonly IReportService _reports;
        public CustomerCityChartViewComponent(IReportService reports) => _reports = reports;

        public async Task<IViewComponentResult> InvokeAsync(int top = 8) => View(await _reports.GetCustomersByCityAsync(top, HttpContext.RequestAborted));
    }

    public sealed class ToDoPriorityChartViewComponent : ViewComponent
    {
        private readonly IReportService _reports;
        public ToDoPriorityChartViewComponent(IReportService reports) => _reports = reports;

        public async Task<IViewComponentResult> InvokeAsync() => View(await _reports.GetToDosByPriorityAsync(HttpContext.RequestAborted));
    }

    public sealed class ActivityListViewComponent : ViewComponent
    {
        private readonly IReportService _reports;
        public ActivityListViewComponent(IReportService reports) => _reports = reports;

        public async Task<IViewComponentResult> InvokeAsync() => View(await _reports.GetActivitiesAsync(HttpContext.RequestAborted));
    }

    public sealed class LatestMessagesViewComponent : ViewComponent
    {
        private readonly IInboxService _inbox;
        public LatestMessagesViewComponent(IInboxService inbox) => _inbox = inbox;

        public async Task<IViewComponentResult> InvokeAsync(int take = 5) => View(await _inbox.GetLatestMessagesAsync(take, HttpContext.RequestAborted));
    }

    public sealed class LatestOrdersViewComponent : ViewComponent
    {
        private readonly IOrderService _orders;
        public LatestOrdersViewComponent(IOrderService orders) => _orders = orders;

        public async Task<IViewComponentResult> InvokeAsync(int take = 5) => View(await _orders.GetLatestAsync(take, HttpContext.RequestAborted));
    }

    /// <summary>Son görevler. Dashboard kartı "Default", sağ paneldeki liste "Sidebar" view'ını kullanıyor.</summary>
    public sealed class LatestToDosViewComponent : ViewComponent
    {
        private readonly IToDoService _todos;
        public LatestToDosViewComponent(IToDoService todos) => _todos = todos;

        public async Task<IViewComponentResult> InvokeAsync(int take = 5, string view = "Default") =>
            View(view, await _todos.GetLatestAsync(take, HttpContext.RequestAborted));
    }

    public sealed class StoreStatisticsViewComponent : ViewComponent
    {
        private readonly IReportService _reports;
        public StoreStatisticsViewComponent(IReportService reports) => _reports = reports;

        public async Task<IViewComponentResult> InvokeAsync() => View(await _reports.GetStoreStatisticsAsync(ct: HttpContext.RequestAborted));
    }
}
