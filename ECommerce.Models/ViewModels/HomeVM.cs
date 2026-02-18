using System.Collections.Generic;
using ECommerce.Models.Models;

namespace ECommerce.Models.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Slider> Sliders { get; set; } = new List<Slider>();

        public IEnumerable<Product> NewProducts { get; set; } = new List<Product>();
        public IEnumerable<Product> DiscountedProducts { get; set; } = new List<Product>();
        public IEnumerable<Product> BestSellingProducts { get; set; } = new List<Product>();
        public IEnumerable<Product> AllProducts { get; set; } = new List<Product>();

        public IEnumerable<Category> MainCategories { get; set; } = new List<Category>();
        public IEnumerable<Category> SubCategories { get; set; } = new List<Category>();

        // Selected category state for filtering
        public int? SelectedMainCategoryId { get; set; }
        public int? SelectedSubCategoryId { get; set; }

        // Products filtered by selected subcategory (or main category)
        public IEnumerable<Product> FilteredProducts { get; set; } = new List<Product>();

        // 🔽 Search & Filter Parameters
        public string? SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } // "newest", "price-asc", "price-desc", "discount"
        public string? SelectedBrand { get; set; }
        public bool? InStockOnly { get; set; } = false;

        // Available brands for dropdown filter
        public IEnumerable<string> AvailableBrands { get; set; } = new List<string>();

        // Pagination info
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalProducts { get; set; }

        // Category Filter Model
        public CategoryFilterVM CategoryFilter { get; set; } = new CategoryFilterVM();
    }

    // For the category filter partial
    public class CategoryFilterVM
    {
        public IEnumerable<Category> MainCategories { get; set; } = new List<Category>();
        public IEnumerable<Category> SubCategories { get; set; } = new List<Category>();

        public int? SelectedMainCategoryId { get; set; }
        public int? SelectedSubCategoryId { get; set; }
    }

    // For reusable product sections partial
    public class ProductSectionVM
    {
        public string Title { get; set; } = string.Empty;
        public IEnumerable<Product> Products { get; set; } = new List<Product>();

        // Optional CSS hook for styling each section differently
        public string CssClass { get; set; } = string.Empty;
    }
}