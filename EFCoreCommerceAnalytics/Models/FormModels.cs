using System.ComponentModel.DataAnnotations;
using EFCoreCommerceAnalytics.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EFCoreCommerceAnalytics.Models
{
    // Formlar entity'lere değil bu modellere bağlanıyor. Böylece formda olmayan alanlar (ör. sipariş toplamı)
    // dışarıdan gönderilemiyor ve doğrulama kuralları tek yerde duruyor.

    public sealed class CategoryInput
    {
        [Display(Name = "Kategori adı")]
        [Required(ErrorMessage = "{0} zorunlu.")]
        [StringLength(100, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Durum")]
        public bool Status { get; set; } = true;
    }

    public sealed class ProductInput
    {
        [Display(Name = "Ürün adı")]
        [Required(ErrorMessage = "{0} zorunlu.")]
        [StringLength(150, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Fiyat")]
        [Range(typeof(decimal), "0.01", "10000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "{0} 0,01 ile 10.000.000 arasında olmalı.")]
        public decimal Price { get; set; }

        [Display(Name = "Stok")]
        [Range(0, 1_000_000, ErrorMessage = "{0} 0 ile 1.000.000 arasında olmalı.")]
        public int Stock { get; set; }

        [Display(Name = "Kategori")]
        [Range(1, int.MaxValue, ErrorMessage = "Bir kategori seçin.")]
        public int CategoryId { get; set; }

        public Product ToEntity() => new() { ProductName = Name, ProductPrice = Price, ProductStock = Stock, CategoryId = CategoryId };

        public static ProductInput From(Product p) => new() { Name = p.ProductName, Price = p.ProductPrice, Stock = p.ProductStock, CategoryId = p.CategoryId };
    }

    public sealed class CustomerInput
    {
        [Display(Name = "Ad")]
        [Required(ErrorMessage = "{0} zorunlu.")]
        [StringLength(50, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Soyad")]
        [Required(ErrorMessage = "{0} zorunlu.")]
        [StringLength(50, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Şehir")]
        [Required(ErrorMessage = "{0} zorunlu.")]
        [StringLength(50, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "İlçe")]
        [StringLength(50, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
        public string? District { get; set; }

        [Display(Name = "Bakiye")]
        [Range(typeof(decimal), "0", "100000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "{0} eksi olamaz.")]
        public decimal Balance { get; set; }

        [Display(Name = "Resim adresi")]
        [Url(ErrorMessage = "Geçerli bir adres girin (https://...).")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public Customer ToEntity() => new()
        {
            CustomerFirstName = FirstName, CustomerLastName = LastName, CustomerCity = City,
            CustomerDistrict = District, CustomerBalance = Balance, CustomerImageUrl = ImageUrl
        };

        public static CustomerInput From(Customer c) => new()
        {
            FirstName = c.CustomerFirstName, LastName = c.CustomerLastName, City = c.CustomerCity,
            District = c.CustomerDistrict, Balance = c.CustomerBalance, ImageUrl = c.CustomerImageUrl
        };
    }

    public sealed class OrderInput
    {
        [Display(Name = "Müşteri")]
        [Range(1, int.MaxValue, ErrorMessage = "Bir müşteri seçin.")]
        public int CustomerId { get; set; }

        [Display(Name = "Ürün")]
        [Range(1, int.MaxValue, ErrorMessage = "Bir ürün seçin.")]
        public int ProductId { get; set; }

        [Display(Name = "Adet")]
        [Range(1, 10_000, ErrorMessage = "{0} 1 ile 10.000 arasında olmalı.")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Durum")]
        public string? Status { get; set; }
    }

    public sealed class ToDoInput
    {
        [Display(Name = "Açıklama")]
        [Required(ErrorMessage = "{0} zorunlu.")]
        [StringLength(200, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Tamamlandı")]
        public bool Done { get; set; }

        [Display(Name = "Öncelik")]
        [Required(ErrorMessage = "Bir öncelik seçin.")]
        public string Priority { get; set; } = ToDoPriorities.First;
    }

    /// <summary>Ekleme ve düzenleme formları aynı view'ı kullanıyor; Id doluysa düzenleme.</summary>
    public sealed class FormPage<TInput>
    {
        public FormPage(TInput input, int? id = null)
        {
            Input = input;
            Id = id;
        }

        public TInput Input { get; }
        public int? Id { get; }
        public bool IsEdit => Id.HasValue;
        /// <summary>Formdaki seçim kutusunun seçenekleri (ör. ürün formunda kategoriler).</summary>
        public IReadOnlyList<SelectListItem> Options { get; init; } = Array.Empty<SelectListItem>();
    }

    /// <summary>Sipariş formu: müşteri ve ürün listeleri ile (düzenlemede) durum seçenekleri.</summary>
    public sealed class OrderFormPage
    {
        public OrderInput Input { get; init; } = new();
        public int? Id { get; init; }
        public bool IsEdit => Id.HasValue;
        public IReadOnlyList<Customer> Customers { get; init; } = Array.Empty<Customer>();
        public IReadOnlyList<Product> Products { get; init; } = Array.Empty<Product>();
        public IReadOnlyList<string> Statuses => OrderStatuses.All;
    }
}
