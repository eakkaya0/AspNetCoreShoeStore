using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün adı boş bırakılamaz.")]
    [MaxLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir.")]
    [MinLength(3, ErrorMessage = "Ürün adı en az 3 karakter olmalıdır.")]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    // 🔽 Kategori İlişkisi
    [Required(ErrorMessage = "Kategori seçilmelidir.")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; } = null!;

    // 🔽 Marka
    [Required(ErrorMessage = "Marka adı boş bırakılamaz.")]
    [MaxLength(100, ErrorMessage = "Marka adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Marka")]
    public string Brand { get; set; } = string.Empty;

    // 🔽 Fiyatlandırma (Discount-Based)
    [Required(ErrorMessage = "Liste fiyatı girilmelidir.")]
    [Range(0.01, 1000000, ErrorMessage = "Liste fiyatı 0.01 ile 1.000.000 arasında olmalıdır.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Liste Fiyatı")]
    public decimal ListPrice { get; set; }

    [Range(0, 100, ErrorMessage = "İndirim oranı 0 ile 100 arasında olmalıdır.")]
    [Display(Name = "İndirim Oranı (%)")]
    public decimal? DiscountRate { get; set; }

    // Calculated property - not mapped to database
    [NotMapped]
    [Display(Name = "İndirimli Fiyat")]
    public decimal? DiscountedPrice
    {
        get
        {
            if (DiscountRate.HasValue && DiscountRate.Value > 0)
            {
                return ListPrice - (ListPrice * DiscountRate.Value / 100);
            }
            return null;
        }
    }

    // 🔽 Stok
    [Required(ErrorMessage = "Stok miktarı girilmelidir.")]
    [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya pozitif olmalıdır.")]
    [Display(Name = "Stok Miktarı")]
    public int StockQuantity { get; set; } = 0;

    // 🔽 Ayakkabı Özellikleri
    [MaxLength(100)]
    [Display(Name = "Renk")]
    public string? Color { get; set; }

    [Display(Name = "Mevcut Numaralar")]
    public string? AvailableSizes { get; set; } // Örn: "36,37,38,39,40"

    // 🔽 Görsel
    [MaxLength(500)]
    [Display(Name = "Ürün Görseli")]
    public string? ImageUrl { get; set; }

    // 🔽 Durum
    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Silinmiş")]
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedDate { get; set; }

    // Varyantlar (beden bazlı stok) ve görseller
    public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}