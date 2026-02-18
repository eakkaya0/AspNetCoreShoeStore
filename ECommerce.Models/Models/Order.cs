using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ECommerce.Models.Identity;

namespace ECommerce.Models.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal => TotalAmount + ShippingCost;

        // Guest kullanıcı bilgileri
        public string? GuestEmail { get; set; }
        public string? GuestFirstName { get; set; }
        public string? GuestLastName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestCity { get; set; }
        public string? GuestAddress { get; set; }
        public string? GuestNotes { get; set; }

        // Kayıtlı kullanıcı bilgisi
        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        // Ödeme bilgileri
        public string? PaymentReference { get; set; }
        public DateTime? PaymentDate { get; set; }
        public bool IsPaid { get; set; } = false;

        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Helper property
        [NotMapped]
        public bool IsGuestOrder => string.IsNullOrEmpty(ApplicationUserId);
    }

    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Preparing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        Refunded = 7
    }
}
