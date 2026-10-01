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

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<ToDo> ToDos => Set<ToDo>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Para alanları önceden de decimal(18,2) olarak oluşmuştu; burada açıkça yazarak
            // EF'in "kesinlik belirtilmedi" uyarısını kaldırıyor ve şemayı sabitliyoruz.
            modelBuilder.Entity<Product>().Property(p => p.ProductPrice).HasPrecision(18, 2);
            modelBuilder.Entity<Customer>().Property(c => c.CustomerBalance).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.TotalPrice).HasPrecision(18, 2);

            // Kategori silinince ürünleri, müşteri ya da ürün silinince siparişleri sessizce silinmesin.
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category).WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Product).WithMany(p => p.Orders)
                .HasForeignKey(o => o.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer).WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);

            // Rapor ekranlarının sık filtrelediği alanlar.
            modelBuilder.Entity<Order>().Property(o => o.SaleStatus).HasMaxLength(50);
            modelBuilder.Entity<ToDo>().Property(t => t.Priority).HasMaxLength(50);
            modelBuilder.Entity<Order>().HasIndex(o => o.OrderDate);
            modelBuilder.Entity<Order>().HasIndex(o => o.SaleStatus);
            modelBuilder.Entity<ToDo>().HasIndex(t => t.Priority);
        }
    }
}
