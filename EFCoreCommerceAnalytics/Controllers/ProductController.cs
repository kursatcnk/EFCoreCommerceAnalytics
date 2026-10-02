using EFCoreCommerceAnalytics.Models;
using EFCoreCommerceAnalytics.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class ProductController : AppController
    {
        private readonly IProductService _products;
        private readonly ICategoryService _categories;

        public ProductController(IProductService products, ICategoryService categories)
        {
            _products = products;
            _categories = categories;
        }

        public async Task<IActionResult> ProductList(string? search, int page = 1, CancellationToken ct = default)
        {
            var model = await _products.GetPagedAsync(search, page, ct);
            return IsAjaxRequest ? PartialView("_ProductRows", model) : View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct(CancellationToken ct) =>
            View("ProductForm", await FormAsync(new ProductInput(), null, ct));

        [HttpPost]
        public async Task<IActionResult> AddProduct([Bind(Prefix = "Input")] ProductInput input, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View("ProductForm", await FormAsync(input, null, ct));

            await _products.CreateAsync(input.ToEntity(), ct);
            FlashSuccess($"\"{input.Name}\" eklendi.");
            return RedirectToAction(nameof(ProductList));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(int id, CancellationToken ct)
        {
            var product = await _products.GetAsync(id, ct);
            if (product is null) return NotFound();

            return View("ProductForm", await FormAsync(ProductInput.From(product), id, ct));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(int id, [Bind(Prefix = "Input")] ProductInput input, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View("ProductForm", await FormAsync(input, id, ct));

            var result = await _products.UpdateAsync(id, input.ToEntity(), ct);
            if (result.IsNotFound) return NotFound();

            Flash(result, "Ürün güncellendi.");
            return RedirectToAction(nameof(ProductList));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken ct)
        {
            Flash(await _products.DeleteAsync(id, ct), "Ürün silindi.");
            return RedirectToAction(nameof(ProductList));
        }

        private async Task<FormPage<ProductInput>> FormAsync(ProductInput input, int? id, CancellationToken ct)
        {
            var categories = await _categories.GetAllAsync(ct);
            return new FormPage<ProductInput>(input, id)
            {
                Options = categories
                    .Select(c => new SelectListItem(c.Status ? c.CategoryName : $"{c.CategoryName} (pasif)", c.CategoryId.ToString()))
                    .ToList()
            };
        }
    }
}
