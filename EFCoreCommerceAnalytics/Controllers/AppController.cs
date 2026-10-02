using EFCoreCommerceAnalytics.Models;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreCommerceAnalytics.Controllers
{
    /// <summary>Controller'ların ortak yardımcıları: AJAX isteği tespiti ve işlem sonucunu kullanıcıya bildirme.</summary>
    public abstract class AppController : Controller
    {
        public const string SuccessKey = "Success";
        public const string ErrorKey = "Error";

        protected bool IsAjaxRequest => Request.Headers.XRequestedWith == "XMLHttpRequest";

        protected void FlashSuccess(string message) => TempData[SuccessKey] = message;

        protected void FlashError(string message) => TempData[ErrorKey] = message;

        /// <summary>Sonuca göre başarı ya da hata mesajını bir sonraki sayfada gösterilmek üzere saklar.</summary>
        protected void Flash(OperationResult result, string successMessage)
        {
            if (result.Succeeded)
            {
                FlashSuccess(successMessage);
            }
            else
            {
                FlashError(result.Error ?? "İşlem tamamlanamadı.");
            }
        }
    }
}
