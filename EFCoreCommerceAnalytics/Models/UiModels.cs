using System.Globalization;
using EFCoreCommerceAnalytics.Entities;

namespace EFCoreCommerceAnalytics.Models
{
    /// <summary>_Avatar partial'ı için ad ve (varsa) resim adresi.</summary>
    public sealed record AvatarModel(string Name, string? ImageUrl)
    {
        private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

        public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Trim()[..1].ToUpper(Turkish);
    }

    /// <summary>İstatistik sayfasındaki tek bir kart.</summary>
    public sealed record StatCardModel(string Title, string Value, string Note, string Icon);

    /// <summary>Sayfalama bileşeni: hangi action'a gidileceği ve (varsa) arama metni.</summary>
    public sealed record PagerModel(int Page, int TotalPages, string Action, string? Search = null)
    {
        public static PagerModel For<T>(PagedList<T> list, string action) => new(list.Page, list.TotalPages, action, list.Search);
    }

    /// <summary>Aynı tablo görünümünü kullanan görev sayfaları (Concat ve Union örnekleri).</summary>
    public sealed record ToDoTablePage(string Title, IReadOnlyList<ToDo> Items);
}
