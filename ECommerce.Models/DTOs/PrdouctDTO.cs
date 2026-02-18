namespace ECommerce.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public int CategoryId { get; set; }
        
        // Kategori bilgileri
        public string CategoryName { get; set; } // "Ana Kategori / Alt Kategori"
        public string ParentCategoryName { get; set; } // Sadece ana kategori
        public string SubCategoryName { get; set; } // Sadece alt kategori
        
        // Fiyatlandırma
        public decimal ListPrice { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal? DiscountedPrice { get; set; }
        
        // Stok ve özellikler
        public int StockQuantity { get; set; }
        public string Color { get; set; }
        public string AvailableSizes { get; set; }
        
        // Görsel
        public string ImageUrl { get; set; }
        
        // Durum
        public bool IsActive { get; set; }
        
        // Tarih
        public DateTime CreatedDate { get; set; }
    }
}