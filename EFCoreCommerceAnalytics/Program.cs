using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Data;
using EFCoreCommerceAnalytics.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("'DefaultConnection' bağlantı dizesi bulunamadı. appsettings.json ya da user-secrets içinde tanımlayın.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddControllersWithViews();

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
