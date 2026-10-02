using EFCoreCommerceAnalytics.Models;
using EFCoreCommerceAnalytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class OrderController : AppController
    {
        private readonly IOrderService _orders;
        private readonly ICustomerService _customers;
        private readonly IProductService _products;

        public OrderController(IOrderService orders, ICustomerService customers, IProductService products)
        {
            _orders = orders;
            _customers = customers;
            _products = products;
        }

        /// <summary>İptal edilmemiş siparişler, numara sırasıyla.</summary>
        public Task<IActionResult> OrderList(string? search, int page = 1, CancellationToken ct = default) =>
            ListAsync(OrderListFilter.Active, newestFirst: false, search, page, "Siparişler", nameof(OrderList), ct);

        /// <summary>İptal edilmemiş siparişler, en yeniden eskiye.</summary>
        public Task<IActionResult> ActiveOrders(string? search, int page = 1, CancellationToken ct = default) =>
            ListAsync(OrderListFilter.Active, newestFirst: true, search, page, "Aktif siparişler (en yeni önce)", nameof(ActiveOrders), ct);

        /// <summary>Eski adres; menüdeki ve dışarıdaki bağlantılar bozulmasın diye yönlendiriliyor.</summary>
        [HttpGet]
        public IActionResult DeliveredAndActiveOrders(int page = 1) => RedirectToActionPermanent(nameof(ActiveOrders), new { page });

        /// <summary>İptal edilenler. Arama kutusu ve sayfalama vardı ama action bunları desteklemiyordu; artık destekliyor.</summary>
        public Task<IActionResult> CanceledOrders(string? search, int page = 1, CancellationToken ct = default) =>
            ListAsync(OrderListFilter.Cancelled, newestFirst: true, search, page, "İptal edilen siparişler", nameof(CanceledOrders), ct);

        [HttpGet]
        public async Task<IActionResult> CreateOrder(CancellationToken ct) =>
            View("OrderForm", await FormAsync(new OrderInput(), null, ct));

        [HttpPost]
        public async Task<IActionResult> CreateOrder([Bind(Prefix = "Input")] OrderInput input, CancellationToken ct)
        {
            if (ModelState.IsValid)
            {
                var result = await _orders.CreateAsync(input.CustomerId, input.ProductId, input.Quantity, ct);
                if (result.Succeeded)
                {
                    FlashSuccess("Sipariş oluşturuldu.");
                    return RedirectToAction(nameof(OrderList));
                }

                ModelState.AddModelError(string.Empty, result.Error!);
            }

            return View("OrderForm", await FormAsync(input, null, ct));
        }

        /// <summary>Menüde "İşlem Yap" olarak bağlanan bu sayfanın view'ı hiç yoktu; açılınca hata veriyordu.</summary>
        [HttpGet]
        public async Task<IActionResult> UpdateOrder(int id, CancellationToken ct)
        {
            var order = await _orders.GetAsync(id, ct);
            if (order is null) return NotFound();

            var input = new OrderInput { CustomerId = order.CustomerId, ProductId = order.ProductId, Quantity = order.OrderCount, Status = order.SaleStatus };
            return View("OrderForm", await FormAsync(input, id, ct));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(int id, [Bind(Prefix = "Input")] OrderInput input, CancellationToken ct)
        {
            if (ModelState.IsValid)
            {
                var result = await _orders.UpdateAsync(id, input.CustomerId, input.ProductId, input.Quantity, input.Status, ct);
                if (result.IsNotFound) return NotFound();
                if (result.Succeeded)
                {
                    FlashSuccess($"#{id} numaralı sipariş güncellendi.");
                    return RedirectToAction(nameof(OrderList));
                }

                ModelState.AddModelError(string.Empty, result.Error!);
            }

            return View("OrderForm", await FormAsync(input, id, ct));
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id, CancellationToken ct)
        {
            Flash(await _orders.CancelAsync(id, ct), $"#{id} numaralı sipariş iptal edildi.");
            return RedirectToAction(nameof(OrderList));
        }

        /// <summary>Sipariş formundaki müşteri arama penceresi için.</summary>
        [HttpGet]
        public async Task<IActionResult> SearchCustomers(string? term, CancellationToken ct)
        {
            var customers = await _customers.SearchAsync(term, ct: ct);
            return Json(customers.Select(c => new { id = c.CustomerId, name = c.FullName, city = c.CustomerCity }));
        }

        /// <summary>Sipariş formundaki ürün arama penceresi için.</summary>
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string? term, CancellationToken ct)
        {
            var products = await _products.SearchAsync(term, ct: ct);
            return Json(products.Select(p => new { id = p.ProductId, name = p.ProductName, price = p.ProductPrice }));
        }

        private async Task<IActionResult> ListAsync(OrderListFilter filter, bool newestFirst, string? search, int page, string title, string action, CancellationToken ct)
        {
            var model = await _orders.GetPagedAsync(filter, search, page, newestFirst, ct);
            ViewData["ShowActions"] = filter == OrderListFilter.Active;
            ViewData["Title"] = title;
            ViewData["ListAction"] = action;
            return IsAjaxRequest ? PartialView("_OrderRows", model) : View("Orders", model);
        }

        private async Task<OrderFormPage> FormAsync(OrderInput input, int? id, CancellationToken ct) => new()
        {
            Input = input,
            Id = id,
            Customers = await _customers.GetAllAsync(ct),
            Products = await _products.GetAllAsync(ct)
        };
    }
}
