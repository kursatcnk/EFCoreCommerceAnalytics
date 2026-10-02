using EFCoreCommerceAnalytics.Infrastructure;

namespace EFCoreCommerceAnalytics.Tests
{
    public class FlexibleDecimalModelBinderTests
    {
        [Theory]
        [InlineData("1899.50", 1899.50)]   // number input'ların gönderdiği biçim
        [InlineData("1899,50", 1899.50)]   // Türkçe ondalık virgül
        [InlineData("1.899,50", 1899.50)]  // Türkçe binlik ayırıcıyla
        [InlineData("1,899.50", 1899.50)]  // İngilizce binlik ayırıcıyla
        [InlineData(" 42 ", 42)]
        [InlineData("0", 0)]
        public void Parses_both_separator_styles(string raw, double expected)
        {
            Assert.True(FlexibleDecimalModelBinder.TryParse(raw, out var value));
            Assert.Equal((decimal)expected, value);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12,34,56.7.8")]
        public void Rejects_garbage(string raw)
        {
            Assert.False(FlexibleDecimalModelBinder.TryParse(raw, out _));
        }
    }
}
