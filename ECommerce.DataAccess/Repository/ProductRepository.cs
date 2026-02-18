using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerce.DataAccess.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ECommerceDbContext _context;

    public ProductRepository(ECommerceDbContext context) 
        : base(context)
    {
        _context = context;
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    // Override GetAllAsync to include Category navigation property with parent
    public new async Task<IEnumerable<Product>> GetAllAsync(Expression<Func<Product, bool>>? filter = null)
    {
        IQueryable<Product> query = _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory);

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.ToListAsync();
    }

    // Override GetAsync to include Category navigation property with parent
    public new async Task<Product?> GetAsync(Expression<Func<Product, bool>> filter, bool tracked = true)
    {
        IQueryable<Product> query = _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory);

        if (!tracked)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(filter);
    }

    /// <summary>
    /// Ürün detay sayfası için: Category, ParentCategory, ProductVariants, ProductImages dahil.
    /// </summary>
    public async Task<Product?> GetWithDetailsAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c!.ParentCategory)
            .Include(p => p.ProductVariants)
            .Include(p => p.ProductImages)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted && p.IsActive);
    }

    public async Task<Product?> GetWithDetailsForAdminAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c!.ParentCategory)
            .Include(p => p.ProductVariants)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    /// <summary>
    /// Arama, filtreleme ve sıralama ile ürün listesi getir.
    /// </summary>
    public async Task<(IEnumerable<Product> Products, int TotalCount)> SearchFilterSortAsync(
        int? categoryId = null,
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? brand = null,
        bool inStockOnly = false,
        string sortBy = "newest", // "newest", "price-asc", "price-desc", "discount"
        int pageNumber = 1,
        int pageSize = 12)
    {
        IQueryable<Product> query = _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory)
            .AsNoTracking();

        // Base filter: active and not deleted
        query = query.Where(p => p.IsActive && !p.IsDeleted);

        // Category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Search filter (Name, Description, Brand)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string searchLower = searchTerm.ToLower().Trim();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                p.Brand.ToLower().Contains(searchLower));
        }

        // Price range filter
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.ListPrice >= minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.ListPrice <= maxPrice.Value);
        }

        // Brand filter
        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.Brand.ToLower() == brand.ToLower());
        }

        // Stock filter
        if (inStockOnly)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        // Get total count before pagination
        int totalCount = await query.CountAsync();

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "price-asc" => query.OrderBy(p => p.ListPrice),
            "price-desc" => query.OrderByDescending(p => p.ListPrice),
            "discount" => query
                .Where(p => p.DiscountRate.HasValue && p.DiscountRate.Value > 0)
                .OrderByDescending(p => p.DiscountRate),
            _ => query.OrderByDescending(p => p.CreatedDate) // Default: newest
        };

        // Pagination
        int skip = (pageNumber - 1) * pageSize;
        var products = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    /// <summary>
    /// Tüm aktif ürünlerden tekil brand listesi getir.
    /// </summary>
    public async Task<IEnumerable<string>> GetDistinctBrandsAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive && !p.IsDeleted)
            .Select(p => p.Brand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync();
    }
}
