namespace EFCoreCommerceAnalytics.Entities
{
    /// <summary>
    /// Görev öncelikleri. Önceden sorgularda "Ikincil" ve "Besincil" gibi farklı yazımlar
    /// kullanıldığı için ilgili sayfalar hiç kayıt bulamıyordu; artık herkes bu sabitleri kullanıyor.
    /// </summary>
    public static class ToDoPriorities
    {
        public const string First = "Birincil";
        public const string Second = "İkincil";
        public const string Third = "Üçüncül";
        public const string Fourth = "Dördüncül";
        public const string Fifth = "Beşincil";

        public static readonly IReadOnlyList<string> All = new[] { First, Second, Third, Fourth, Fifth };
    }
}
