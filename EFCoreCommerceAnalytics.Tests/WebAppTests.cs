using System.Net;
using System.Text.RegularExpressions;
using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreCommerceAnalytics.Tests
{
    /// <summary>Uygulamayı SQL Server yerine InMemory veritabanı ve örnek verilerle ayağa kaldırır.</summary>
    public sealed class AppFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                foreach (var descriptor in services.Where(d =>
                             d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                             d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>)).ToList())
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            });
        }

        public async Task SeedAsync()
        {
            using var scope = Services.CreateScope();
            await DevelopmentSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
        }
    }

    public class WebAppTests : IClassFixture<AppFactory>, IAsyncLifetime
    {
        private readonly AppFactory _factory;
        private readonly HttpClient _client;

        public WebAppTests(AppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        public Task InitializeAsync() => _factory.SeedAsync();

        public Task DisposeAsync() => Task.CompletedTask;

        [Theory]
        [InlineData("/")]
        [InlineData("/Dashboard/Statistics")]
        [InlineData("/Category/CategoryList")]
        [InlineData("/Category/CreateCategory")]
        [InlineData("/Product/ProductList")]
        [InlineData("/Product/AddProduct")]
        [InlineData("/Customer/CustomerList")]
        [InlineData("/Customer/CreateCustomer")]
        [InlineData("/Customer/HighBalanceCustomers")]
        [InlineData("/Customer/CustomersByCity")]
        [InlineData("/Customer/TopCitiesWithTopCustomers")]
        [InlineData("/Order/OrderList")]
        [InlineData("/Order/ActiveOrders")]
        [InlineData("/Order/CanceledOrders")]
        [InlineData("/Order/CreateOrder")]
        [InlineData("/Order/UpdateOrder/1")]
        [InlineData("/Message/MessageList")]
        [InlineData("/Message/Detail/1")]
        [InlineData("/ToDo/Create")]
        [InlineData("/ToDo/PrimaryAggregate")]
        [InlineData("/ToDo/PrimaryChunks")]
        [InlineData("/ToDo/PrimaryAndSecondary")]
        [InlineData("/ToDo/FourthAndFifth")]
        public async Task Every_page_renders(string url)
        {
            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/Customer/CustomersNormalBalance", "/Customer/HighBalanceCustomers")]
        [InlineData("/Order/DeliveredAndActiveOrders", "/Order/ActiveOrders")]
        [InlineData("/ToDo/CreateSingleTodo", "/ToDo/Create")]
        public async Task Old_urls_redirect(string oldUrl, string newUrl)
        {
            var response = await _client.GetAsync(oldUrl);
            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.StartsWith(newUrl, response.Headers.Location!.OriginalString);
        }

        [Fact]
        public async Task Unknown_record_returns_404_page()
        {
            var response = await _client.GetAsync("/Customer/UpdateCustomer/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("bulunamadı", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Delete_without_antiforgery_token_is_rejected()
        {
            var response = await _client.PostAsync("/Category/DeleteCategory/1", new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Delete_is_not_reachable_with_get()
        {
            var response = await _client.GetAsync("/Category/DeleteCategory/1");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Category_names_are_encoded_in_ajax_rows()
        {
            var page = await _client.GetStringAsync("/Category/CreateCategory");
            var token = Regex.Match(page, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;

            var post = await _client.PostAsync("/Category/CreateCategory", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Input.Name"] = "<img src=x onerror=alert(1)>"
            }));
            Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);

            var request = new HttpRequestMessage(HttpMethod.Get, "/Category/CategoryList?search=onerror");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            var rows = await (await _client.SendAsync(request)).Content.ReadAsStringAsync();

            Assert.Contains("&lt;img src=x onerror=alert(1)&gt;", rows);
            Assert.DoesNotContain("<img src=x", rows);
            Assert.DoesNotContain("<html", rows);
        }

        [Fact]
        public async Task Turkish_text_is_not_turned_into_entities()
        {
            var html = await _client.GetStringAsync("/Customer/CustomersByCity");
            Assert.Contains("Şehirlere göre", html);
            Assert.DoesNotContain("&#x15E;", html);
        }
    }
}
