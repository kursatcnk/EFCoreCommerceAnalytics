using System.Globalization;

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
}
