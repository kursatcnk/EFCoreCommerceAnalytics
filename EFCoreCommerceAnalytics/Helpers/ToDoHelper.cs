namespace EFCoreCommerceAnalytics.Helpers
{
    public static class ToDoHelper
    {
        // Status true ise 'completed', false ise 'form-check' class döndürür
        public static string GetLiClass(bool status)
        {
            return status ? "completed" : "form-check";
        }

        // Checkbox input için checked durumunu ayarlar
        public static string GetCheckboxChecked(bool status)
        {
            return status ? "checked" : "";
        }
    }
}
