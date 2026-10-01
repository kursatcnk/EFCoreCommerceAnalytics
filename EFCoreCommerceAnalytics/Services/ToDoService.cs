using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Services
{
    /// <summary>
    /// Görev sayfaları LINQ operatörlerini (Chunk, Aggregate, Union, Concat) göstermek için yazılmıştı.
    /// Bu örnekler korunuyor; sorgular sadece burada toplanıp doğru öncelik değerlerini kullanacak şekilde düzeltildi.
    /// </summary>
    public interface IToDoService
    {
        Task<IReadOnlyList<ToDo>> GetLatestAsync(int take, CancellationToken ct = default);
        Task<IReadOnlyList<ToDo>> GetByPriorityAsync(string priority, CancellationToken ct = default);
        Task<IReadOnlyList<ToDo[]>> GetFirstPriorityInChunksAsync(int chunkSize, CancellationToken ct = default);
        Task<(IReadOnlyList<ToDo> Items, string Joined)> GetFirstPriorityWithAggregateAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ToDo>> GetFourthAndFifthUnionAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ToDo>> GetFirstAndSecondConcatAsync(CancellationToken ct = default);
        Task CreateAsync(string description, bool done, string priority, CancellationToken ct = default);
        Task<int> AddRandomSamplesAsync(CancellationToken ct = default);
    }

    public sealed class ToDoService : IToDoService
    {
        private static readonly string[] SampleDescriptions =
        {
            "Raporu oku ve özetle", "E-postaları kontrol et", "Sunum hazırla", "Toplantı notlarını gözden geçir",
            "Kod incelemesi yap", "Yeni özellik ekle", "Hata düzeltmelerini uygula", "Dokümantasyonu güncelle",
            "Müşteri geri bildirimlerini incele", "Takvim güncellemelerini yap"
        };

        private readonly AppDbContext _db;

        public ToDoService(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<ToDo>> GetLatestAsync(int take, CancellationToken ct = default) =>
            await _db.ToDos.AsNoTracking().OrderByDescending(t => t.TodoId).Take(take).ToListAsync(ct);

        public async Task<IReadOnlyList<ToDo>> GetByPriorityAsync(string priority, CancellationToken ct = default) =>
            await _db.ToDos.AsNoTracking().Where(t => t.Priority == priority).OrderBy(t => t.TodoId).ToListAsync(ct);

        /// <summary>Chunk: birincil görevleri eşit büyüklükte gruplara böler.</summary>
        public async Task<IReadOnlyList<ToDo[]>> GetFirstPriorityInChunksAsync(int chunkSize, CancellationToken ct = default)
        {
            var items = await GetByPriorityAsync(ToDoPriorities.First, ct);
            return items.Chunk(chunkSize).ToList();
        }

        /// <summary>Aggregate: birincil görevlerin açıklamalarını tek bir metinde birleştirir.</summary>
        public async Task<(IReadOnlyList<ToDo> Items, string Joined)> GetFirstPriorityWithAggregateAsync(CancellationToken ct = default)
        {
            var items = await GetByPriorityAsync(ToDoPriorities.First, ct);
            var joined = items.Count == 0
                ? string.Empty
                : items.Select(t => t.ToDoDescription).Aggregate((current, next) => current + " | " + next);
            return (items, joined);
        }

        /// <summary>Union: dördüncül ve beşincil görevleri tekrarsız birleştirir.</summary>
        public async Task<IReadOnlyList<ToDo>> GetFourthAndFifthUnionAsync(CancellationToken ct = default)
        {
            var fourth = await GetByPriorityAsync(ToDoPriorities.Fourth, ct);
            var fifth = await GetByPriorityAsync(ToDoPriorities.Fifth, ct);
            return fourth.UnionBy(fifth, t => t.TodoId).ToList();
        }

        /// <summary>Concat: birincil ve ikincil görevleri sırayla art arda ekler.</summary>
        public async Task<IReadOnlyList<ToDo>> GetFirstAndSecondConcatAsync(CancellationToken ct = default)
        {
            var first = await GetByPriorityAsync(ToDoPriorities.First, ct);
            var second = await GetByPriorityAsync(ToDoPriorities.Second, ct);
            return first.Concat(second).ToList();
        }

        public async Task CreateAsync(string description, bool done, string priority, CancellationToken ct = default)
        {
            _db.ToDos.Add(new ToDo { ToDoDescription = description.Trim(), ToDoStatus = done, Priority = priority });
            await _db.SaveChangesAsync(ct);
        }

        /// <summary>AddRange örneği: 5-10 arası rastgele görev ekler ve eklenen sayıyı döndürür.</summary>
        public async Task<int> AddRandomSamplesAsync(CancellationToken ct = default)
        {
            var count = Random.Shared.Next(5, 11);
            var todos = Enumerable.Range(0, count).Select(_ => new ToDo
            {
                ToDoDescription = SampleDescriptions[Random.Shared.Next(SampleDescriptions.Length)],
                ToDoStatus = Random.Shared.Next(2) == 1,
                Priority = ToDoPriorities.All[Random.Shared.Next(ToDoPriorities.All.Count)]
            }).ToList();

            await _db.ToDos.AddRangeAsync(todos, ct);
            await _db.SaveChangesAsync(ct);
            return count;
        }
    }
}
