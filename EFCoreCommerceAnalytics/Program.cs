using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Data;
using EFCoreCommerceAnalytics.Infrastructure;
using EFCoreCommerceAnalytics.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.WebEncoders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("'DefaultConnection' bağlantı dizesi bulunamadı. appsettings.json ya da user-secrets içinde tanımlayın.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddControllersWithViews(options =>
{
    // Bütün POST istekleri anti-forgery token ister; formlardaki token form tag helper'ı tarafından ekleniyor.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.ModelBinderProviders.Insert(0, new FlexibleDecimalModelBinderProvider());
});

// Razor varsayılan olarak Latin dışındaki karakterleri entity'ye çevirir (ı -> &#x131;); Türkçe metin olduğu gibi yazılsın.
builder.Services.Configure<WebEncoderOptions>(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IToDoService, ToDoService>();
builder.Services.AddScoped<IInboxService, InboxService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Geliştirmede bekleyen migration'ları uygula ve boş veritabanını örnek verilerle doldur.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DevelopmentSeeder.SeedAsync(db);
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Para, tarih ve sayılar sunucunun diline göre değil, her zaman Türkçe biçimde gösterilsin (₺, 1.234,50).
var turkish = CultureInfo.GetCultureInfo("tr-TR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(turkish),
    SupportedCultures = new[] { turkish },
    SupportedUICultures = new[] { turkish }
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
