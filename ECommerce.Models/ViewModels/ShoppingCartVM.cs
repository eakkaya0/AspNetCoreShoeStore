using ECommerce.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels;

/// <summary>
/// Sepet sayfası için ana ViewModel
/// </summary>
public class ShoppingCartVM
{
    /// <summary>
    /// Sepetteki ürünler listesi
    /// </summary>
    public List<CartItemVM> CartItems { get; set; } = new();

    /// <summary>
    /// Sepet toplamı (kargo hariç)
    /// </summary>
    [Display(Name = "Sepet Toplamı")]
    public decimal CartTotal { get; set; }

    /// <summary>
    /// Toplam ürün adedi
    /// </summary>
    [Display(Name = "Toplam Adet")]
    public int TotalItems { get; set; }

    /// <summary>
    /// Kargo ücreti (sabit veya hesaplanabilir)
    /// </summary>
    [Display(Name = "Kargo")]
    public decimal ShippingCost { get; set; } = 0;

    /// <summary>
    /// Genel toplam (sepet + kargo)
    /// </summary>
    [Display(Name = "Genel Toplam")]
    public decimal GrandTotal => CartTotal + ShippingCost;
}

/// <summary>
/// Sepetteki tek bir ürün için ViewModel
/// </summary>
public class CartItemVM
{
    public int Id { get; set; }

    /// <summary>
    /// Ürün ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Ürün adı
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Ürün görseli
    /// </summary>
    public string? ProductImageUrl { get; set; }

    /// <summary>
    /// Varyant ID (beden gibi)
    /// </summary>
    public int? ProductVariantId { get; set; }

    /// <summary>
    /// Varyant bilgisi (beden, renk vb.)
    /// </summary>
    public string? VariantInfo { get; set; }

    /// <summary>
    /// Sepetteki adet
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Adet 1 ile 1000 arasında olmalıdır.")]
    public int Count { get; set; }

    /// <summary>
    /// Birim fiyat (indirimli fiyat varsa onu kullanır)
    /// </summary>
    [Display(Name = "Birim Fiyat")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Toplam fiyat (adet × birim fiyat)
    /// </summary>
    [Display(Name = "Toplam Fiyat")]
    public decimal TotalPrice => Count * UnitPrice;

    /// <summary>
    /// Stok durumu
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Stokta var mı?
    /// </summary>
    public bool InStock => StockQuantity > 0;

    /// <summary>
    /// Stokta yeterli miktarda var mı?
    /// </summary>
    public bool HasEnoughStock => StockQuantity >= Count;
}

/// <summary>
/// Sepete ekleme işlemi için kullanılan ViewModel
/// </summary>
public class AddToCartVM
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    public int ProductId { get; set; }

    /// <summary>
    /// Varyant ID (opsiyonel - ürün varyantı yoksa null)
    /// </summary>
    public int? ProductVariantId { get; set; }

    [Range(1, 100, ErrorMessage = "Adet 1 ile 100 arasında olmalıdır.")]
    public int Quantity { get; set; } = 1;
}
