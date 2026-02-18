using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Models.Models;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Identity;

namespace ECommerceWeb.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 📋 LİSTELEME
        public async Task<IActionResult> Index()
        {
         var categories = (await _unitOfWork.Category.GetAllAsync(c => !c.IsDeleted))
        .OrderBy(c => c.ParentCategoryId ?? c.Id)
       .ThenBy(c => c.ParentCategoryId.HasValue)
       .ThenBy(c => c.DisplayOrder)
           .ToList();



            return View(categories);
        }

        // ➕ YENİ KATEGORİ (GET)
        public async Task<IActionResult> Create()
        {
            await LoadParentCategories();
            return View();
        }

        // ➕ YENİ KATEGORİ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (category.Name == category.DisplayOrder?.ToString())
                ModelState.AddModelError("Name", "Kategori Adı ve Sıra aynı olamaz.");

            if (!ModelState.IsValid)
            {
                await LoadParentCategories();
                return View(category);
            }

            // Aynı isim ve parent ile aktif kategori kontrolü
            var exists = (await _unitOfWork.Category.GetAllAsync(c => c.Name == category.Name 
                               && c.ParentCategoryId == category.ParentCategoryId 
                               && !c.IsDeleted))
                .Any();

            if (exists)
            {
                ModelState.AddModelError("Name", "Bu kategori zaten mevcut.");
                await LoadParentCategories();
                return View(category);
            }

            // Silinmiş kategoriyi geri aktifleştir
            var deletedCategory = await _unitOfWork.Category
                .GetAsync(c => c.Name == category.Name 
                            && c.ParentCategoryId == category.ParentCategoryId 
                            && c.IsDeleted, tracked: true);

            if (deletedCategory != null)
            {
                deletedCategory.IsDeleted = false;
                deletedCategory.IsActive = category.IsActive;
                deletedCategory.DisplayOrder = category.DisplayOrder;
                _unitOfWork.Category.Update(deletedCategory);
            }
            else
            {
                category.IsDeleted = false;
                await _unitOfWork.Category.AddAsync(category);
            }

            await _unitOfWork.SaveAsync();
            TempData["SuccessMessage"] = "Kategori başarıyla kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        // ✏️ DÜZENLE (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _unitOfWork.Category.GetAsync(c => c.Id == id && !c.IsDeleted);
            if (category == null) return NotFound();

            await LoadParentCategories(id);
            return View(category);
        }

        // ✏️ DÜZENLE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            if (category.ParentCategoryId == id)
                ModelState.AddModelError("ParentCategoryId", "Kategori kendi alt kategorisi olamaz.");

            if (!ModelState.IsValid)
            {
                await LoadParentCategories(id);
                return View(category);
            }

            // Duplicate kontrolü
            var duplicate = (await _unitOfWork.Category
                .GetAllAsync(c => c.Name == category.Name 
                               && c.ParentCategoryId == category.ParentCategoryId 
                               && c.Id != id 
                               && !c.IsDeleted))
                .Any();

            if (duplicate)
            {
                ModelState.AddModelError("Name", "Bu isimde başka bir kategori mevcut.");
                await LoadParentCategories(id);
                return View(category);
            }

            // Döngüsel referans kontrolü
            if (category.ParentCategoryId.HasValue && 
                await HasCircularReference(id, category.ParentCategoryId.Value))
            {
                ModelState.AddModelError("ParentCategoryId", "Döngüsel kategori ilişkisi oluşturulamaz.");
                await LoadParentCategories(id);
                return View(category);
            }

            var existing = await _unitOfWork.Category.GetAsync(c => c.Id == id && !c.IsDeleted);
            if (existing == null) return NotFound();

            existing.Name = category.Name;
            existing.DisplayOrder = category.DisplayOrder;
            existing.ParentCategoryId = category.ParentCategoryId;
            existing.IsActive = category.IsActive;

            _unitOfWork.Category.Update(existing);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Kategori güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // 🗑️ SİL (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _unitOfWork.Category.GetAsync(c => c.Id == id && !c.IsDeleted);
            if (category == null) return NotFound();

            return View(category);
        }

        // 🗑️ SİL (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _unitOfWork.Category.GetAsync(c => c.Id == id && !c.IsDeleted);
            if (category == null) return NotFound();

            // Alt kategori kontrolü
            var hasSubCategory = (await _unitOfWork.Category
                .GetAllAsync(c => c.ParentCategoryId == id && !c.IsDeleted))
                .Any();

            if (hasSubCategory)
            {
                TempData["ErrorMessage"] = "Bu kategoriye bağlı alt kategoriler var.";
                return RedirectToAction(nameof(Index));
            }

            category.IsDeleted = true;
            _unitOfWork.Category.Update(category);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Kategori silindi.";
            return RedirectToAction(nameof(Index));
        }

        // 🔧 YARDIMCI METODLAR
        private async Task LoadParentCategories(int? excludeId = null)
        {
            var parents = (await _unitOfWork.Category
                .GetAllAsync(c => c.ParentCategoryId == null && !c.IsDeleted))
                .Where(c => !excludeId.HasValue || c.Id != excludeId.Value);

            ViewBag.ParentCategories = parents
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
                .ToList();
        }

        private async Task<bool> HasCircularReference(int categoryId, int parentId)
        {
            var parent = await _unitOfWork.Category.GetAsync(c => c.Id == parentId && !c.IsDeleted);
            if (parent == null) return false;
            if (parent.ParentCategoryId == categoryId) return true;
            if (parent.ParentCategoryId.HasValue)
                return await HasCircularReference(categoryId, parent.ParentCategoryId.Value);
            return false;
        }
    }
}