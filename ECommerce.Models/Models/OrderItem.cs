using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        // Varyant (beden) desteği
        public int? ProductVariantId { get; set; }
        [ForeignKey("ProductVariantId")]
        public ProductVariant? ProductVariant { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
