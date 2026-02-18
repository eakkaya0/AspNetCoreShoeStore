using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ECommerce.Models.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı boş bırakılamaz.")]
    [MaxLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
    [MinLength(2, ErrorMessage = "Kategori adı en az 2 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sıra alanı boş bırakılamaz.")]
    [Range(1, 100, ErrorMessage = "Sıra değeri 1 ile 100 arasında olmalıdır.")]
    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // 🔽 Üst kategori
    [Display(Name = "Üst Kategori")]
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    // 🔽 Alt kategoriler (HİYERARŞİ İÇİN GEREKLİ)
    public List<Category> SubCategories { get; set; } = new();

    // 🔽 Ürün bağlantısı (İLERİDE SİLME KONTROLÜ İÇİN)
   // public List<Product> Products { get; set; } = new();

    // 🔽 Soft Delete
    public bool IsDeleted { get; set; } = false;
}

