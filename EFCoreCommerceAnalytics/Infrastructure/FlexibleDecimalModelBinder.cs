using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EFCoreCommerceAnalytics.Infrastructure
{
    /// <summary>
    /// Arayüz tr-TR kültüründe çalışıyor; ama tarayıcıdaki number input'ları değeri her zaman "1899.50" gibi
    /// noktayla gönderiyor. Varsayılan bağlayıcı bunu Türkçe kültürde 189950 diye okuyordu. Bu bağlayıcı hem
    /// "1899.50" hem "1.899,50" hem de "1899,50" yazımını doğru çözüyor.
    /// </summary>
    public sealed class FlexibleDecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, bindingContext.ValueProvider.GetValue(bindingContext.ModelName));

            if (string.IsNullOrWhiteSpace(value))
            {
                if (bindingContext.ModelType == typeof(decimal))
                {
                    bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Bir sayı girin.");
                }

                return Task.CompletedTask;
            }

            if (TryParse(value, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Geçerli bir sayı girin.");
            }

            return Task.CompletedTask;
        }

        public static bool TryParse(string raw, out decimal result)
        {
            var text = raw.Trim().Replace(" ", string.Empty);
            var lastComma = text.LastIndexOf(',');
            var lastDot = text.LastIndexOf('.');

            if (lastComma >= 0 && lastDot >= 0)
            {
                // İkisi de varsa sonda olan ondalık ayırıcıdır: "1.899,50" ya da "1,899.50".
                text = lastComma > lastDot
                    ? text.Replace(".", string.Empty).Replace(',', '.')
                    : text.Replace(",", string.Empty);
            }
            else if (lastComma >= 0)
            {
                text = text.Replace(',', '.');
            }

            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }
    }

    public sealed class FlexibleDecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
            context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?)
                ? new FlexibleDecimalModelBinder()
                : null;
    }
}
