using EFCoreCommerceAnalytics.Entities;
using EFCoreCommerceAnalytics.Models;
using EFCoreCommerceAnalytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.Controllers
{
    /// <summary>
    /// Görevler. Liste sayfaları LINQ operatörlerini göstermek için var: Aggregate, Chunk, Concat, Union.
    /// Eski adresler yeni action'lara yönlendiriliyor.
    /// </summary>
    public class ToDoController : AppController
    {
        private readonly IToDoService _todos;

        public ToDoController(IToDoService todos) => _todos = todos;

        [HttpGet]
        public IActionResult Create() => View(new ToDoInput());

        [HttpPost]
        public async Task<IActionResult> Create(ToDoInput input, CancellationToken ct)
        {
            if (!ToDoPriorities.All.Contains(input.Priority))
            {
                ModelState.AddModelError(nameof(input.Priority), "Geçersiz öncelik.");
            }

            if (!ModelState.IsValid) return View(input);

            await _todos.CreateAsync(input.Description, input.Done, input.Priority, ct);
            FlashSuccess("Görev eklendi.");
            return RedirectToAction(nameof(Create));
        }

        /// <summary>
        /// Rastgele örnek görev ekler. Eskiden bu bir GET adresiydi; menüdeki "Yeni Görev Ekle" linkine her tıklayışta
        /// ve hatta tarayıcı sayfayı önceden yüklediğinde veritabanına görev ekleniyordu. Artık yalnızca form ile POST.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddSamples(CancellationToken ct)
        {
            var count = await _todos.AddRandomSamplesAsync(ct);
            FlashSuccess($"{count} örnek görev eklendi.");
            return RedirectToAction(nameof(Create));
        }

        /// <summary>Aggregate: birincil görevlerin açıklamalarını tek metinde birleştirir.</summary>
        public async Task<IActionResult> PrimaryAggregate(CancellationToken ct)
        {
            var (items, joined) = await _todos.GetFirstPriorityWithAggregateAsync(ct);
            ViewData["Joined"] = joined;
            return View(items);
        }

        /// <summary>Chunk: birincil görevleri üçerli gruplar hâlinde gösterir.</summary>
        public async Task<IActionResult> PrimaryChunks(CancellationToken ct) =>
            View(await _todos.GetFirstPriorityInChunksAsync(3, ct));

        /// <summary>Concat: birincil ve ikincil görevler art arda.</summary>
        public async Task<IActionResult> PrimaryAndSecondary(CancellationToken ct) =>
            View("ToDoTable", new ToDoTablePage("Birincil + ikincil görevler (Concat)", await _todos.GetFirstAndSecondConcatAsync(ct)));

        /// <summary>Union: dördüncül ve beşincil görevler tekrarsız.</summary>
        public async Task<IActionResult> FourthAndFifth(CancellationToken ct) =>
            View("ToDoTable", new ToDoTablePage("Dördüncül + beşincil görevler (Union)", await _todos.GetFourthAndFifthUnionAsync(ct)));

        // Eski adresler
        [HttpGet] public IActionResult CreateSingleTodo() => RedirectToActionPermanent(nameof(Create));
        [HttpGet] public IActionResult CreateToDo() => RedirectToActionPermanent(nameof(Create));
        [HttpGet] public IActionResult ShowPriortyNumberOne() => RedirectToActionPermanent(nameof(PrimaryAggregate));
        [HttpGet] public IActionResult ShowPrimaryTodosInChunks() => RedirectToActionPermanent(nameof(PrimaryChunks));
        [HttpGet] public IActionResult ShowPrimaryAndSecondaryTodos() => RedirectToActionPermanent(nameof(PrimaryAndSecondary));
        [HttpGet] public IActionResult ShowFourthAndFifthUnion() => RedirectToActionPermanent(nameof(FourthAndFifth));
    }
}
