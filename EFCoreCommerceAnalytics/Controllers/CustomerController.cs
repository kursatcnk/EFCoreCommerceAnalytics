using EFCoreCommerceAnalytics.Models;
using EFCoreCommerceAnalytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class CustomerController : AppController
    {
        /// <summary>"Yüksek bakiyeli müşteriler" raporunun eşiği.</summary>
        public const decimal HighBalanceThreshold = 1000m;

        private readonly ICustomerService _customers;

        public CustomerController(ICustomerService customers) => _customers = customers;

        public async Task<IActionResult> CustomerList(string? search, int page = 1, CancellationToken ct = default)
        {
            var model = await _customers.GetPagedAsync(search, page, ct);
            return IsAjaxRequest ? PartialView("_CustomerRows", model) : View(model);
        }

        [HttpGet]
        public IActionResult CreateCustomer() => View("CustomerForm", new FormPage<CustomerInput>(new CustomerInput()));

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([Bind(Prefix = "Input")] CustomerInput input, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View("CustomerForm", new FormPage<CustomerInput>(input));

            await _customers.CreateAsync(input.ToEntity(), ct);
            FlashSuccess($"{input.FirstName} {input.LastName} eklendi.");
            return RedirectToAction(nameof(CustomerList));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCustomer(int id, CancellationToken ct)
        {
            var customer = await _customers.GetAsync(id, ct);
            if (customer is null) return NotFound();

            return View("CustomerForm", new FormPage<CustomerInput>(CustomerInput.From(customer), id));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(int id, [Bind(Prefix = "Input")] CustomerInput input, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View("CustomerForm", new FormPage<CustomerInput>(input, id));

            var result = await _customers.UpdateAsync(id, input.ToEntity(), ct);
            if (result.IsNotFound) return NotFound();

            Flash(result, "Müşteri güncellendi.");
            return RedirectToAction(nameof(CustomerList));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id, string? returnUrl, CancellationToken ct)
        {
            Flash(await _customers.DeleteAsync(id, ct), "Müşteri silindi.");
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(CustomerList));
        }

        /// <summary>Bakiyesi eşiğin üzerinde olan müşteriler (eski adıyla CustomersNormalBalance).</summary>
        public async Task<IActionResult> HighBalanceCustomers(int page = 1, CancellationToken ct = default)
        {
            ViewData["Threshold"] = HighBalanceThreshold;
            return View(await _customers.GetWithBalanceAtLeastAsync(HighBalanceThreshold, page, ct));
        }

        [HttpGet]
        public IActionResult CustomersNormalBalance(int page = 1) => RedirectToActionPermanent(nameof(HighBalanceCustomers), new { page });

        public async Task<IActionResult> CustomersByCity(CancellationToken ct) =>
            View(await _customers.GetCustomerCountsByCityAsync(ct));

        public async Task<IActionResult> TopCitiesWithTopCustomers(CancellationToken ct) =>
            View(await _customers.GetTopCitiesWithTopCustomersAsync(3, ct));
    }
}
