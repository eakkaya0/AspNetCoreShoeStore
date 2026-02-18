using ECommerce.Models.Models;

namespace ECommerce.Models.ViewModels;

/// <summary>
/// Ürün detay sayfası için ViewModel: ürün, beden/varyant listesi, görseller, benzer ürünler.
/// </summary>
public class ProductDetailVM
{
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Beden bazlı stok (varyant varsa kullanılır; yoksa AvailableSizes'tan parse edilir, stok tek).
    /// </summary>
    public List<SizeStockItem> SizeStocks { get; set; } = new();

    /// <summary>
    /// Ana + ek görseller (galeri). En az 1 (Product.ImageUrl), ProductImages varsa onlar da eklenir.
    /// </summary>
    public List<string> ImageUrls { get; set; } = new();

    /// <summary>
    /// Aynı kategoriden 4 benzer ürün.
    /// </summary>
    public List<Product> SimilarProducts { get; set; } = new();

    /// <summary>
    /// Stokta son X ürün mesajı (örn. 5 ve altı için gösterilir).
    /// </summary>
    public int? LastStockWarningThreshold { get; set; }
}

public class SizeStockItem
{
    public string Size { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int? VariantId { get; set; }
}
