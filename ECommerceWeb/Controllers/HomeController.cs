using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ECommerce.Models.Models;
using ECommerce.Models.ViewModels;
using ECommerce.DataAccess.Repository.IRepository;

namespace ECommerce.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IUnitOfWork unitOfWork, ILogger<HomeController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: /Home/Index
        // Simple search by product name, brand, or description
        public async Task<IActionResult> Index(string? searchTerm, int? mainCategoryId, int? subCategoryId)
        {
            // 1. Sliders (from admin)
            var sliders = (await _unitOfWork.Slider
                    .GetAllAsync(s => s.IsActive))
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            // 2. All products (base set for rest of logic) - sadece aktif ve silinmemiş ürünler
            var allProducts = (await _unitOfWork.Product
                    .GetAllAsync(p => !p.IsDeleted && p.IsActive))
                .ToList();

            // 3. New products (latest 10)
            var newProducts = allProducts
                .OrderByDescending(p => p.CreatedDate)
                .Take(10)
                .ToList();

            // 4. Discounted products (based on DiscountRate)
            var discountedProducts = allProducts
                .Where(p => p.DiscountRate.HasValue && p.DiscountRate.Value > 0)
                .OrderByDescending(p => p.DiscountRate)
                .ThenByDescending(p => p.CreatedDate)
                .ToList();

            // 5. Best-selling products
            var topSellingIds = (await _unitOfWork.OrderItem.GetTopSellingProductIdsAsync(10)).ToList();
            List<Product> bestSellingProducts;
            if (topSellingIds.Count > 0)
            {
                var byId = allProducts.ToDictionary(p => p.Id);
                bestSellingProducts = topSellingIds
                    .Where(id => byId.ContainsKey(id))
                    .Select(id => byId[id])
                    .ToList();

                // Satışı olmayan ürünlerden eksik kalanları tamamla (liste hep 10 ürün olsun)
                if (bestSellingProducts.Count < 10)
                {
                    var filler = allProducts
                        .Where(p => !topSellingIds.Contains(p.Id))
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(10 - bestSellingProducts.Count)
                        .ToList();
                    bestSellingProducts.AddRange(filler);
                }
            }
            else
            {
                // Satış yoksa random göster
                bestSellingProducts = allProducts
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(10)
                    .ToList();
            }

            // 6. Categories (main & sub)
            var allCategories = (await _unitOfWork.Category
                    .GetAllAsync(c => c.IsActive && !c.IsDeleted))
                .ToList();

            var mainCategories = allCategories
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder ?? int.MaxValue)
                .ThenBy(c => c.Name)
                .ToList();

            // Get subcategories if main category is selected
            var subCategories = new List<Category>();
            if (mainCategoryId.HasValue)
            {
                subCategories = allCategories
                    .Where(c => c.ParentCategoryId == mainCategoryId)
                    .OrderBy(c => c.DisplayOrder ?? int.MaxValue)
                    .ThenBy(c => c.Name)
                    .ToList();
            }

            // 7. Search and category filtering
            IEnumerable<Product> filteredProducts = allProducts;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.ToLower().Trim();
                filteredProducts = allProducts.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                    p.Brand.ToLower().Contains(searchLower))
                    .OrderByDescending(p => p.CreatedDate)
                    .ToList();
            }

            // Apply category filter
            if (subCategoryId.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.CategoryId == subCategoryId).ToList();
            }
            else if (mainCategoryId.HasValue)
            {
                // Filter by main category (include all products from main and sub categories)
                var mainCatIds = new List<int> { mainCategoryId.Value };
                mainCatIds.AddRange(subCategories.Select(s => s.Id));
                filteredProducts = filteredProducts.Where(p => mainCatIds.Contains(p.CategoryId)).ToList();
            }

            // Build the HomeVM
            var vm = new HomeVM
            {
                Sliders = sliders,
                NewProducts = newProducts,
                DiscountedProducts = discountedProducts,
                BestSellingProducts = bestSellingProducts,
                AllProducts = allProducts,

                MainCategories = mainCategories,
                SubCategories = subCategories,

                SearchTerm = searchTerm,
                SelectedMainCategoryId = mainCategoryId,
                SelectedSubCategoryId = subCategoryId,
                FilteredProducts = filteredProducts.ToList(),

                // Category Filter
                CategoryFilter = new CategoryFilterVM
                {
                    MainCategories = mainCategories,
                    SubCategories = subCategories,
                    SelectedMainCategoryId = mainCategoryId,
                    SelectedSubCategoryId = subCategoryId
                }
            };

            return View(vm);
        }
        
    }
}