using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Controllers
{
    /// <summary>
    ///  ToDo işlemlerini yöneten MVC Controller.
    ///  EF Core ile veritabanına asenkron CRUD ve Aggregate örnekleri içerir.
    /// </summary>
    public class ToDoController : Controller
    {
        private readonly AppDbContext _context;

        public ToDoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        ///  Örnek görevleri veritabanına ekler (Range) ve rastgele içerik üretir.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateToDo()
        {
            string[] descriptions = new string[]
            {
                "Raporu oku ve özetle",
                "E-postaları kontrol et",
                "Sunum hazırla",
                "Toplantı notlarını gözden geçir",
                "Kod incelemesi yap",
                "Yeni özellik ekle",
                "Hata düzeltmelerini uygula",
                "Dokümantasyonu güncelle",
                "Müşteri geri bildirimlerini incele",
                "Takvim güncellemelerini yap"
            };

            string[] priorities = new string[]
            {
                "Birincil",
                "İkincil",
                "Üçüncül",
                "Dördüncül",
                "Beşincil"
            };

            var random = new Random();
            var todos = new List<ToDo>();

            int taskCount = random.Next(5, 11);
            for (int i = 0; i < taskCount; i++)
            {
                var todo = new ToDo
                {
                    ToDoDescription = descriptions[random.Next(descriptions.Length)],
                    ToDoStatus = random.Next(2) == 0 ? false : true,
                    Priority = priorities[random.Next(priorities.Length)]
                };
                todos.Add(todo);
            }

            await _context.ToDos.AddRangeAsync(todos);
            await _context.SaveChangesAsync();

            ViewBag.Message = $"{taskCount} adet rastgele görev başarıyla eklendi.";

            return View();
        }

        /// <summary>
        ///  Tek bir görev eklemek için GET formu gösterir.
        /// </summary>
        [HttpGet]
        public IActionResult CreateSingleTodo()
        {
            return View();
        }

        /// <summary>
        ///  Tek bir görevi veritabanına ekler (POST).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSingleTodo(ToDo model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _context.ToDos.AddAsync(model);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Görev başarıyla eklendi.";

            return View();
        }

        /// <summary>
        ///  "Birincil" önceliğe sahip görevleri toplar ve ekrana gösterir (düz liste).
        ///  Chunk kullanımı view’da yapılacak şekilde.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ShowPrimaryTodosInChunks()
        {
            var birincilTodos = await _context.ToDos
                .Where(t => t.Priority == "Birincil")
                .ToListAsync();

            // View'a düz liste gönderiyoruz, view chunk mantığını uygular
            return View(birincilTodos);
        }

        /// <summary>
        ///  "Birincil" önceliğe sahip görevleri toplar ve Aggregate ile gösterir.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ShowPriortyNumberOne()
        {
            var birincilTodos = await _context.ToDos
                .Where(t => t.Priority == "Birincil")
                .ToListAsync();

            var aggregatedDescriptions = birincilTodos.Any()
                ? birincilTodos.Select(t => t.ToDoDescription)
                               .Aggregate((current, next) => current + " | " + next)
                : "Birincil görev bulunamadı.";

            ViewBag.BirincilCount = birincilTodos.Count;
            ViewBag.AggregatedDescriptions = aggregatedDescriptions;

            return View(birincilTodos);
        }
        [HttpGet]
        public async Task<IActionResult> ShowFourthAndFifthUnion()
        {
            // Dördüncül görevler
            var fourthTodos = await _context.ToDos
                .Where(t => t.Priority == "Dördüncül")
                .ToListAsync();

            // Beşincil görevler
            var fifthTodos = await _context.ToDos
                .Where(t => t.Priority == "Besincil")
                .ToListAsync();

            // Union ile birleştiriyoruz
            var combinedTodos = fourthTodos.Union(fifthTodos).ToList();

            ViewBag.TotalCount = combinedTodos.Count;

            return View(combinedTodos);
        }

        [HttpGet]
        public async Task<IActionResult> ShowPrimaryAndSecondaryTodos()
        {
            var primaryTodos = await _context.ToDos
                .Where(t => t.Priority == "Birincil")
                .ToListAsync();

            var secondaryTodos = await _context.ToDos
                .Where(t => t.Priority == "Ikincil")
                .ToListAsync();

            // Concat ile birleştiriyoruz
            var combinedTodos = primaryTodos.Concat(secondaryTodos).ToList();

            ViewBag.TotalCount = combinedTodos.Count;
            return View(combinedTodos);
        }

    }
}
