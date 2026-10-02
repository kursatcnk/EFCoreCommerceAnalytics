using EFCoreCommerceAnalytics.Models;
using EFCoreCommerceAnalytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.Controllers
{
    public class CategoryController : AppController
    {
        private readonly ICategoryService _categories;

        public CategoryController(ICategoryService categories) => _categories = categories;

        /// <summary>Liste. Arama kutusundan gelen AJAX isteğinde yalnızca tablo satırları (partial) döner.</summary>
        public async Task<IActionResult> CategoryList(string? search, int page = 1, CancellationToken ct = default)
        {
            var model = await _categories.GetPagedAsync(search, page, ct);
            return IsAjaxRequest ? PartialView("_CategoryRows", model) : View(model);
        }

        [HttpGet]
        public IActionResult CreateCategory() => View("CategoryForm", new FormPage<CategoryInput>(new CategoryInput()));

        [HttpPost]
        public async Task<IActionResult> CreateCategory([Bind(Prefix = "Input")] CategoryInput input, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View("CategoryForm", new FormPage<CategoryInput>(input));

            await _categories.CreateAsync(input.Name, ct);
            FlashSuccess($"\"{input.Name}\" kategorisi eklendi.");
            return RedirectToAction(nameof(CategoryList));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCategory(int id, CancellationToken ct)
        {
            var category = await _categories.GetAsync(id, ct);
            if (category is null) return NotFound();

            return View("CategoryForm", new FormPage<CategoryInput>(new CategoryInput { Name = category.CategoryName, Status = category.Status }, id));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(int id, [Bind(Prefix = "Input")] CategoryInput input, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View("CategoryForm", new FormPage<CategoryInput>(input, id));

            var result = await _categories.UpdateAsync(id, input.Name, input.Status, ct);
            if (result.IsNotFound) return NotFound();

            Flash(result, "Kategori güncellendi.");
            return RedirectToAction(nameof(CategoryList));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct)
        {
            Flash(await _categories.DeleteAsync(id, ct), "Kategori silindi.");
            return RedirectToAction(nameof(CategoryList));
        }
    }
}
