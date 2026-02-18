using ECommerce.Models.Models;

namespace ECommerce.Models.ViewModels
{
    public class OrderSummaryVM
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal GrandTotal { get; set; }
        public bool IsPaid { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public List<OrderItemVM> OrderItems { get; set; } = new();
    }

    public class OrderItemVM
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string? VariantInfo { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
