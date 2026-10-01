using EFCoreCommerceAnalytics.Context;
using EFCoreCommerceAnalytics.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreCommerceAnalytics.Data
{
    /// <summary>
    /// Geliştirme ortamında boş bir veritabanını örnek verilerle doldurur; böylece proje klonlandıktan
    /// hemen sonra dashboard ve raporlar anlamlı bir şey gösterir. Sabit tohumlu Random kullanıldığı için
    /// her kurulumda aynı veri oluşur. Tabloda kayıt varsa hiçbir şeye dokunmaz.
    /// </summary>
    public static class DevelopmentSeeder
    {
        public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
        {
            if (await db.Categories.AnyAsync(ct) || await db.Customers.AnyAsync(ct))
            {
                return;
            }

            var random = new Random(2025);

            var categories = new[] { "Elektronik", "Ev & Yaşam", "Giyim", "Kitap", "Spor", "Kozmetik" }
                .Select(name => new Category { CategoryName = name, Status = true })
                .ToList();

            var productNames = new Dictionary<string, (string Name, decimal Price)[]>
            {
                ["Elektronik"] = new[] { ("Kablosuz Kulaklık", 1899m), ("Akıllı Saat", 4599m), ("Bluetooth Hoparlör", 1299m), ("Dizüstü Bilgisayar", 32999m), ("Tablet", 11499m) },
                ["Ev & Yaşam"] = new[] { ("Kahve Makinesi", 3499m), ("Robot Süpürge", 8999m), ("Masa Lambası", 649m), ("Nevresim Takımı", 1199m) },
                ["Giyim"] = new[] { ("Mont", 2799m), ("Spor Ayakkabı", 2299m), ("Kot Pantolon", 899m), ("Sweatshirt", 749m) },
                ["Kitap"] = new[] { ("Roman Seti", 459m), ("Yazılım Mimarisi Kitabı", 389m), ("Çocuk Kitabı", 149m) },
                ["Spor"] = new[] { ("Yoga Matı", 399m), ("Dambıl Seti", 1499m), ("Bisiklet Kaskı", 999m) },
                ["Kozmetik"] = new[] { ("Parfüm", 1649m), ("Cilt Bakım Seti", 899m), ("Saç Kurutma Makinesi", 1349m) }
            };

            var products = new List<Product>();
            foreach (var category in categories)
            {
                foreach (var (name, price) in productNames[category.CategoryName])
                {
                    products.Add(new Product { ProductName = name, ProductPrice = price, ProductStock = random.Next(5, 250), Category = category });
                }
            }

            var firstNames = new[] { "Ayşe", "Mehmet", "Zeynep", "Ahmet", "Elif", "Mustafa", "Fatma", "Emre", "Merve", "Can", "Selin", "Burak", "Deniz", "Ece", "Kaan" };
            var lastNames = new[] { "Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız", "Aydın", "Öztürk", "Arslan", "Doğan" };
            var places = new[]
            {
                ("İstanbul", "Kadıköy"), ("İstanbul", "Beşiktaş"), ("İstanbul", "Üsküdar"), ("Ankara", "Çankaya"), ("Ankara", "Keçiören"),
                ("İzmir", "Karşıyaka"), ("İzmir", "Bornova"), ("Bursa", "Nilüfer"), ("Antalya", "Muratpaşa"), ("Eskişehir", "Tepebaşı"), ("Sakarya", "Serdivan")
            };

            var customers = Enumerable.Range(0, 40).Select(_ =>
            {
                var (city, district) = places[random.Next(places.Length)];
                return new Customer
                {
                    CustomerFirstName = firstNames[random.Next(firstNames.Length)],
                    CustomerLastName = lastNames[random.Next(lastNames.Length)],
                    CustomerCity = city,
                    CustomerDistrict = district,
                    CustomerBalance = Math.Round((decimal)(random.NextDouble() * 5000), 2)
                };
            }).ToList();

            var statuses = OrderStatuses.All;
            var today = DateTime.Today;
            var orders = Enumerable.Range(0, 220).Select(_ =>
            {
                var product = products[random.Next(products.Count)];
                var count = random.Next(1, 5);
                return new Order
                {
                    Product = product,
                    Customer = customers[random.Next(customers.Count)],
                    OrderCount = count,
                    UnitPrice = product.ProductPrice,
                    TotalPrice = product.ProductPrice * count,
                    OrderDate = today.AddDays(-random.Next(0, 45)).AddMinutes(random.Next(8 * 60, 22 * 60)),
                    SaleStatus = statuses[random.Next(statuses.Count)]
                };
            }).ToList();

            var todoTexts = new[] { "Stok sayımını tamamla", "Kargo firmasıyla görüş", "Kampanya görsellerini onayla", "İade taleplerini incele", "Aylık satış raporunu hazırla", "Tedarikçi faturalarını kontrol et", "Yeni ürün açıklamalarını yaz", "Müşteri yorumlarını yanıtla" };
            var todos = todoTexts.Select((text, i) => new ToDo
            {
                ToDoDescription = text,
                ToDoStatus = i % 3 == 0,
                Priority = ToDoPriorities.All[i % ToDoPriorities.All.Count]
            }).ToList();

            var messages = new[]
            {
                new Message { Title = "Sipariş gecikmesi", SenderNameSurname = "Ayşe Yılmaz", Content = "Siparişim üç gündür kargoda görünüyor, ne zaman teslim edilir?", DateTime = today.AddHours(-3), IsRead = false },
                new Message { Title = "Fatura talebi", SenderNameSurname = "Mehmet Kaya", Content = "Geçen ayki siparişim için kurumsal fatura rica ediyorum.", DateTime = today.AddDays(-1), IsRead = false },
                new Message { Title = "Ürün sorusu", SenderNameSurname = "Zeynep Demir", Content = "Kablosuz kulaklığın pil ömrü ne kadar?", DateTime = today.AddDays(-2), IsRead = true },
                new Message { Title = "İade", SenderNameSurname = "Emre Şahin", Content = "Ayakkabının numarası küçük geldi, değişim yapabilir miyiz?", DateTime = today.AddDays(-4), IsRead = true }
            };

            var activities = new[]
            {
                new Activity { ActivityTitle = "Stok güncellendi", ActivityDescription = "Elektronik kategorisinde 12 ürünün stoğu yenilendi.", ActivityTime = new TimeOnly(9, 15) },
                new Activity { ActivityTitle = "Kampanya başladı", ActivityDescription = "Spor ürünlerinde hafta sonu indirimi yayında.", ActivityTime = new TimeOnly(11, 40) },
                new Activity { ActivityTitle = "Toplu sipariş", ActivityDescription = "Ankara'dan kurumsal bir müşteri 40 adetlik sipariş verdi.", ActivityTime = new TimeOnly(15, 5) }
            };

            db.AddRange(categories);
            db.AddRange(products);
            db.AddRange(customers);
            db.AddRange(orders);
            db.AddRange(todos);
            db.AddRange(messages);
            db.AddRange(activities);
            await db.SaveChangesAsync(ct);

            var latestOrders = orders.OrderByDescending(o => o.OrderDate).Take(5);
            db.AddRange(latestOrders.Select(o => new Notification
            {
                Type = "Order",
                ReferenceId = o.OrderId,
                Title = "Yeni sipariş",
                Content = $"{o.Customer!.FullName}: {o.OrderCount} adet {o.Product!.ProductName}",
                DateTime = o.OrderDate,
                IsRead = false
            }));
            await db.SaveChangesAsync(ct);
        }
    }
}
