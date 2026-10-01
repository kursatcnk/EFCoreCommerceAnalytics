using EFCoreCommerceAnalytics.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Context
{
    public class AppDbContext : DbContext
    {
        // Bağlantı ayarları Program.cs içinde appsettings'ten veriliyor; kodda sabit sunucu adı tutulmuyor.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categiores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ToDo> ToDos { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
