using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models.Models;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [Display(Name = "Sıra")]
    public int DisplayOrder { get; set; }
}
