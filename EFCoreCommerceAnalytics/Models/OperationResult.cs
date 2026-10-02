namespace EFCoreCommerceAnalytics.Models
{
    /// <summary>Kullanıcıya gösterilecek bir nedenle başarısız olabilen işlemlerin sonucu.</summary>
    public sealed record OperationResult(bool Succeeded, string? Error = null, bool IsNotFound = false)
    {
        public static OperationResult Ok() => new(true);
        public static OperationResult Fail(string error) => new(false, error);
        public static OperationResult NotFound() => new(false, "Kayıt bulunamadı.", IsNotFound: true);
    }
}
