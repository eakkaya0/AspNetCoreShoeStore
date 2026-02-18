using System.ComponentModel.DataAnnotations;
using ECommerce.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Models.ViewModels;

public class ProductVM
{
    [ValidateNever]
    public Product Product { get; set; } = new Product();

    [ValidateNever]
    public IEnumerable<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();

    [Display(Name = "Ana görsel (vitrin)")]
    public IFormFile? ImageFile { get; set; }

    /// <summary>
    /// Beden bazlı stok (admin panelinde tablo olarak düzenlenir).
    /// Dolu ise Product.StockQuantity ve AvailableSizes kayıt sırasında buna göre güncellenir.
    /// </summary>
    [ValidateNever]
    public List<VariantEditItem> Variants { get; set; } = new();
}

/// <summary>
/// Admin panelinde tek satır: Beden + Stok (opsiyonel fiyat).
/// </summary>
public class VariantEditItem
{
    public int VariantId { get; set; }
    public string Size { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}