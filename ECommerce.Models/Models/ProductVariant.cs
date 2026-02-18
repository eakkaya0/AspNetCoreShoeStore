using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models.Models;

/// <summary>
/// Ürün varyantı: beden (ve ileride renk) bazlı stok ve opsiyonel fiyat.
/// </summary>
public class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    [Display(Name = "Beden")]
    public string Size { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    [Display(Name = "Stok")]
    public int StockQuantity { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Bedene özel fiyat (null ise ürün fiyatı kullanılır).
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Fiyat (opsiyonel)")]
    public decimal? PriceOverride { get; set; }
}
