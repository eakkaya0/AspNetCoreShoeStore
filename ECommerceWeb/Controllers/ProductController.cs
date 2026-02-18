using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Models.Models;
using ECommerce.Models.ViewModels;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.DTOs;
using ECommerce.Models.Identity;

namespace ECommerceWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        // 📋 INDEX - Product Listing (Yönetim paneli)
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Index()
        {
            return View();
        }

        // 👁️ DETAILS - Ürün detay sayfası (müşteri tarafı)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _unitOfWork.Product.GetWithDetailsAsync(id);
            if (product == null)
                return NotFound();

            var sizeStocks = new List<SizeStockItem>();
            // Her zaman veritabanından gerçek varyant stoklarını oku
            var variantsFromDb = await _unitOfWork.ProductVariant.GetAllAsync(v => v.ProductId == product.Id && v.IsActive);
            if (variantsFromDb.Any())
            {
                sizeStocks = variantsFromDb
                    .OrderBy(v => v.Size)
                    .Select(v => new SizeStockItem
                    {
                        Size = v.Size,
                        Stock = v.StockQuantity,
                        VariantId = v.Id
                    })
                    .ToList();
            }
            else if (!string.IsNullOrWhiteSpace(product.AvailableSizes))
            {
                // Varyant yoksa (eski yapı): genel stok, beden sayısına bölünür
                var sizes = product.AvailableSizes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var stockEach = sizes.Length > 0 ? product.StockQuantity / sizes.Length : product.StockQuantity;
                foreach (var size in sizes)
                    sizeStocks.Add(new SizeStockItem { Size = size, Stock = stockEach, VariantId = null });
            }

            var imageUrls = new List<string>();
            if (!string.IsNullOrEmpty(product.ImageUrl))
                imageUrls.Add(NormalizeImageUrl(product.ImageUrl));
            var galleryImages = (await _unitOfWork.ProductImage.GetAllAsync(i => i.ProductId == id))
                .OrderBy(i => i.DisplayOrder)
                .Select(i => NormalizeImageUrl(i.ImageUrl))
                .ToList();
            imageUrls.AddRange(galleryImages);

            // Aynı kategoriden 4 benzer ürün (mevcut hariç)
            var similarProducts = (await _unitOfWork.Product.GetAllAsync(p =>
                p.CategoryId == product.CategoryId && p.Id != product.Id && !p.IsDeleted && p.IsActive))
                .Take(4)
                .ToList();

            var vm = new ProductDetailVM
            {
                Product = product,
                SizeStocks = sizeStocks,
                ImageUrls = imageUrls.Count > 0 ? imageUrls : new List<string>(), // en az placeholder için boş bırakılabilir
                SimilarProducts = similarProducts,
                LastStockWarningThreshold = 5
            };

            return View(vm);
        }

        // ✏️ UPSERT - GET (Create & Edit)
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Upsert(int? id)
        {
            ProductVM productVM = new ProductVM
            {
                CategoryList = await GetCategorySelectList(),
                Product = new Product(),
                Variants = new List<VariantEditItem>()
            };

            if (id == null || id == 0)
            {
                // CREATE mode: boş bir satır göster (isteğe bağlı)
                productVM.Variants.Add(new VariantEditItem());
                return View(productVM);
            }
            else
            {
                // EDIT mode: ürün + varyantları + galeri görselleri yükle
                var product = await _unitOfWork.Product.GetWithDetailsForAdminAsync(id.Value);
                if (product == null)
                    return NotFound();

                productVM.Product = product;

                var variants = await _unitOfWork.ProductVariant.GetAllAsync(v => v.ProductId == id.Value);
                productVM.Variants = variants
                    .Where(v => v.IsActive)
                    .OrderBy(v => v.Size)
                    .Select(v => new VariantEditItem
                    {
                        VariantId = v.Id,
                        Size = v.Size,
                        StockQuantity = v.StockQuantity
                    })
                    .ToList();
                if (productVM.Variants.Count == 0)
                    productVM.Variants.Add(new VariantEditItem());

                return View(productVM);
            }
        }

        // ✏️ UPSERT - POST (Create & Edit)
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductVM productVM)
        {
            // Validate subcategory selection
            if (productVM.Product.CategoryId > 0)
            {
                var selectedCategory = await _unitOfWork.Category.GetAsync(c => c.Id == productVM.Product.CategoryId && !c.IsDeleted);
                if (selectedCategory != null && selectedCategory.ParentCategoryId == null)
                {
                    ModelState.AddModelError("Product.CategoryId", "Lütfen bir alt kategori seçiniz. Ana kategori seçilemez.");
                }
            }

            // Validate discount rate
            if (productVM.Product.DiscountRate.HasValue && productVM.Product.DiscountRate.Value > 0)
            {
                if (productVM.Product.DiscountRate.Value >= 100)
                {
                    ModelState.AddModelError("Product.DiscountRate", "İndirim oranı 100%'den küçük olmalıdır.");
                }
            }

            if (!ModelState.IsValid)
            {
                productVM.CategoryList = await GetCategorySelectList();
                return View(productVM);
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;

            // Handle Image Upload
            if (productVM.ImageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(productVM.ImageFile.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                // Delete old image if editing and new image uploaded
                if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\', '/'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Ensure directory exists
                if (!Directory.Exists(productPath))
                {
                    Directory.CreateDirectory(productPath);
                }

                // Save new image
                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    await productVM.ImageFile.CopyToAsync(fileStream);
                }

                productVM.Product.ImageUrl = @"\images\product\" + fileName;
            }

            var variantItems = (productVM.Variants ?? new List<VariantEditItem>())
                .Where(v => !string.IsNullOrWhiteSpace(v.Size))
                .Select(v => new { Size = v.Size!.Trim(), v.StockQuantity })
                .ToList();
            var hasVariants = variantItems.Any();

            if (productVM.Product.Id == 0)
            {
                // CREATE
                var exists = (await _unitOfWork.Product.GetAllAsync(p =>
                    p.Name == productVM.Product.Name &&
                    p.Brand == productVM.Product.Brand &&
                    !p.IsDeleted))
                    .Any();

                if (exists)
                {
                    ModelState.AddModelError("Product.Name", "Bu ürün zaten mevcut.");
                    productVM.CategoryList = await GetCategorySelectList();
                    return View(productVM);
                }

                if (hasVariants)
                {
                    productVM.Product.StockQuantity = variantItems.Sum(x => x.StockQuantity);
                    productVM.Product.AvailableSizes = string.Join(",", variantItems.Select(x => x.Size));
                }

                productVM.Product.CreatedDate = DateTime.Now;
                productVM.Product.IsDeleted = false;
                await _unitOfWork.Product.AddAsync(productVM.Product);
                await _unitOfWork.SaveAsync();

                if (hasVariants)
                {
                    foreach (var item in variantItems)
                    {
                        await _unitOfWork.ProductVariant.AddAsync(new ProductVariant
                        {
                            ProductId = productVM.Product.Id,
                            Size = item.Size,
                            StockQuantity = item.StockQuantity
                        });
                    }
                    await _unitOfWork.SaveAsync();
                }
                var galleryFilesCreate = Request.Form.Files
                    .Where(f => string.Equals(f.Name, "GalleryFiles", StringComparison.Ordinal))
                    .Take(3)
                    .ToList();
                await SaveGalleryImagesAsync(productVM.Product.Id, galleryFilesCreate, wwwRootPath);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu.";
            }
            else
            {
                // UPDATE
                var existingProduct = await _unitOfWork.Product.GetAsync(p => p.Id == productVM.Product.Id && !p.IsDeleted);
                if (existingProduct == null)
                    return NotFound();

                var duplicate = (await _unitOfWork.Product.GetAllAsync(p =>
                    p.Name == productVM.Product.Name &&
                    p.Brand == productVM.Product.Brand &&
                    p.Id != productVM.Product.Id &&
                    !p.IsDeleted))
                    .Any();

                if (duplicate)
                {
                    ModelState.AddModelError("Product.Name", "Bu isimde başka bir ürün mevcut.");
                    productVM.CategoryList = await GetCategorySelectList();
                    return View(productVM);
                }

                existingProduct.Name = productVM.Product.Name;
                existingProduct.Description = productVM.Product.Description;
                existingProduct.Brand = productVM.Product.Brand;
                existingProduct.CategoryId = productVM.Product.CategoryId;
                existingProduct.ListPrice = productVM.Product.ListPrice;
                existingProduct.DiscountRate = productVM.Product.DiscountRate;
                existingProduct.Color = productVM.Product.Color;
                existingProduct.IsActive = productVM.Product.IsActive;

                if (hasVariants)
                {
                    existingProduct.StockQuantity = variantItems.Sum(x => x.StockQuantity);
                    existingProduct.AvailableSizes = string.Join(",", variantItems.Select(x => x.Size));
                }
                else
                {
                    existingProduct.StockQuantity = productVM.Product.StockQuantity;
                    existingProduct.AvailableSizes = productVM.Product.AvailableSizes;
                }

                if (productVM.ImageFile != null)
                    existingProduct.ImageUrl = productVM.Product.ImageUrl;

                _unitOfWork.Product.Update(existingProduct);

                // Varyantları güncelle: eskileri sil, yenileri ekle
                var existingVariants = (await _unitOfWork.ProductVariant.GetAllAsync(v => v.ProductId == productVM.Product.Id)).ToList();

                if (hasVariants)
                {
                    var incomingBySize = variantItems
                        .GroupBy(x => x.Size, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                    // DB'de olup formda olmayanları pasifleştir
                    foreach (var v in existingVariants)
                    {
                        if (!incomingBySize.ContainsKey(v.Size))
                        {
                            v.IsActive = false;
                            _unitOfWork.ProductVariant.Update(v);
                        }
                    }

                    // Formdaki varyantları upsert et
                    foreach (var incoming in incomingBySize.Values)
                    {
                        var match = existingVariants.FirstOrDefault(v => string.Equals(v.Size, incoming.Size, StringComparison.OrdinalIgnoreCase));
                        if (match != null)
                        {
                            match.IsActive = true;
                            match.StockQuantity = incoming.StockQuantity;
                            _unitOfWork.ProductVariant.Update(match);
                        }
                        else
                        {
                            await _unitOfWork.ProductVariant.AddAsync(new ProductVariant
                            {
                                ProductId = productVM.Product.Id,
                                Size = incoming.Size,
                                StockQuantity = incoming.StockQuantity,
                                IsActive = true
                            });
                        }
                    }
                }
                else
                {
                    // Varyant sistemi kapatıldıysa tüm varyantları pasifleştir
                    foreach (var v in existingVariants)
                    {
                        v.IsActive = false;
                        _unitOfWork.ProductVariant.Update(v);
                    }
                }

                if (hasVariants)
                {
                    // Aktif varyant stoklarına göre ürün toplam stok bilgisini güncel tut
                    existingProduct.StockQuantity = variantItems.Sum(x => x.StockQuantity);
                    existingProduct.AvailableSizes = string.Join(",", variantItems.Select(x => x.Size));
                }
                var galleryFilesEdit = Request.Form.Files
                    .Where(f => string.Equals(f.Name, "GalleryFiles", StringComparison.Ordinal))
                    .Take(3)
                    .ToList();
                await SaveGalleryImagesAsync(productVM.Product.Id, galleryFilesEdit, wwwRootPath);

                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // 🗑️ DELETE - GET
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Product.GetAsync(p => p.Id == id && !p.IsDeleted);
            if (product == null) return NotFound();

            return View(product);
        }

        // 🗑️ DELETE - POST
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _unitOfWork.Product.GetAsync(p => p.Id == id && !p.IsDeleted);
            if (product == null) return NotFound();

            // Ürün silinirken fiziksel görseli de temizle
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                var imagePath = Path.Combine(wwwRootPath, product.ImageUrl.TrimStart('\\', '/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Soft delete (kayıt veritabanında kalsın, ama sitede görünmesin)
            product.IsDeleted = true;
            _unitOfWork.Product.Update(product);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Ürün silindi.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Detay sayfası galerisi: En fazla 3 görsel. Her kayıtta önce mevcut galeri (dosya + DB) silinir, sonra yeni dosyalar eklenir (birikme olmaz).
        /// </summary>
        private async Task SaveGalleryImagesAsync(int productId, IEnumerable<IFormFile>? galleryFiles, string wwwRootPath)
        {
            var existingImages = (await _unitOfWork.ProductImage.GetAllAsync(i => i.ProductId == productId)).ToList();
            foreach (var img in existingImages)
            {
                if (!string.IsNullOrEmpty(img.ImageUrl))
                {
                    var oldPath = Path.Combine(wwwRootPath, img.ImageUrl.TrimStart('\\', '/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
            }
            if (existingImages.Any())
                await _unitOfWork.ProductImage.RemoveRangeAsync(existingImages);

            var files = galleryFiles?
                .Where(f => f != null && f.Length > 0)
                .Take(3)
                .ToList() ?? new List<IFormFile>();
            if (files.Count == 0) return;

            var productPath = Path.Combine(wwwRootPath, @"images\product");
            if (!Directory.Exists(productPath)) Directory.CreateDirectory(productPath);

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var fullPath = Path.Combine(productPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                    await file.CopyToAsync(stream);

                await _unitOfWork.ProductImage.AddAsync(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = @"\images\product\" + fileName,
                    DisplayOrder = i
                });
            }
        }

        /// <summary>Görsel yolunu tarayıcıda çalışacak formata çevirir (\images\... -> /images/...).</summary>
        private static string NormalizeImageUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            return "/" + url.TrimStart('\\', '/').Replace('\\', '/');
        }

        // 🔧 HELPER METHOD - Get hierarchical category list
        private async Task<IEnumerable<SelectListItem>> GetCategorySelectList()
        {
            var allCategories = (await _unitOfWork.Category.GetAllAsync(c => !c.IsDeleted))
                .OrderBy(c => c.DisplayOrder)
                .ToList();

            var categoryList = new List<SelectListItem>();

            // Get parent categories
            var parentCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();

            foreach (var parent in parentCategories)
            {
                // Get subcategories for this parent
                var subCategories = allCategories
                    .Where(c => c.ParentCategoryId == parent.Id)
                    .OrderBy(c => c.DisplayOrder)
                    .ToList();

                foreach (var sub in subCategories)
                {
                    // Format: "Parent Category / Sub Category"
                    categoryList.Add(new SelectListItem
                    {
                        Text = $"{parent.Name} / {sub.Name}",
                        Value = sub.Id.ToString()
                    });
                }
            }

            return categoryList;
        }

        #region API CALLS

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                // ProductRepository zaten Category.ParentCategory'yi yüklüyor
                var products = await _unitOfWork.Product.GetAllAsync(p => !p.IsDeleted);

                var productDtos = products
                    .OrderBy(p => p.Brand)
                    .ThenBy(p => p.Name)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description ?? "",
                        Brand = p.Brand,
                        CategoryId = p.CategoryId,

                        // Kategori formatı: "Ana Kategori / Alt Kategori"
                        CategoryName = p.Category?.ParentCategory != null
                            ? $"{p.Category.ParentCategory.Name} / {p.Category.Name}"
                            : p.Category?.Name ?? "Kategorisiz",

                        ParentCategoryName = p.Category?.ParentCategory?.Name ?? "",
                        SubCategoryName = p.Category?.Name ?? "",

                        // Fiyat bilgileri
                        ListPrice = p.ListPrice,
                        DiscountRate = p.DiscountRate,
                        DiscountedPrice = p.DiscountRate.HasValue && p.DiscountRate.Value > 0
                            ? p.ListPrice - (p.ListPrice * p.DiscountRate.Value / 100)
                            : null,

                        // Diğer bilgiler
                        StockQuantity = p.StockQuantity,
                        Color = p.Color ?? "",
                        AvailableSizes = p.AvailableSizes ?? "",
                        ImageUrl = p.ImageUrl ?? "",
                        IsActive = p.IsActive,
                        CreatedDate = p.CreatedDate
                    })
                    .ToList();

                return Json(new { data = productDtos });
            }
            catch (Exception ex)
            {
                // Hata durumunda console'a yazdır
                return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        #endregion

    }
    }
   