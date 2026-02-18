using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Models.Models;
namespace ECommerce.DataAccess.Repository.IRepository;


public interface IProductRepository : IRepository<Product>
{
    void Update(Product product);
    Task<Product?> GetWithDetailsAsync(int productId);
    Task<Product?> GetWithDetailsForAdminAsync(int productId);

    /// <summary>
    /// Arama, filtreleme ve sıralama ile ürün listesi getir.
    /// </summary>
    Task<(IEnumerable<Product> Products, int TotalCount)> SearchFilterSortAsync(
        int? categoryId = null,
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? brand = null,
        bool inStockOnly = false,
        string sortBy = "newest",
        int pageNumber = 1,
        int pageSize = 12);

    /// <summary>
    /// Tüm aktif ürünlerden tekil brand listesi getir.
    /// </summary>
    Task<IEnumerable<string>> GetDistinctBrandsAsync();
}
