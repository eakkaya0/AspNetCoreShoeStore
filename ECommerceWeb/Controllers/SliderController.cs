using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Models.Models;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Identity;

namespace ECommerceWeb.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class SliderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SliderController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        // 📋 LİSTELEME
        public async Task<IActionResult> Index()
        {
            var sliders = (await _unitOfWork.Slider.GetAllAsync())
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            return View(sliders);
        }

        // ➕ YENİ SLIDER (GET)
        public IActionResult Create()
        {
            // Varsayılan olarak slider aktif gelsin
            var model = new Slider
            {
                IsActive = true
            };
            return View(model);
        }

        // ➕ YENİ SLIDER (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Slider slider, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(slider);
            }

            // Resim yükleme
            if (imageFile != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                // Tüm slider görselleri için tek klasör: /images/slider
                string sliderPath = Path.Combine(wwwRootPath, @"images\slider");

                if (!Directory.Exists(sliderPath))
                {
                    Directory.CreateDirectory(sliderPath);
                }

                using (var fileStream = new FileStream(Path.Combine(sliderPath, fileName), FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // View tarafında doğrudan kullanılacak sanal yol
                slider.ImageUrl = @"\images\slider\" + fileName;
            }
            else
            {
                slider.ImageUrl = string.Empty;
            }

            await _unitOfWork.Slider.AddAsync(slider);
            await _unitOfWork.SaveAsync();
            
            TempData["SuccessMessage"] = "Slider başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        // ✏️ DÜZENLE (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var slider = await _unitOfWork.Slider.GetAsync(s => s.Id == id);
            if (slider == null) return NotFound();

            return View(slider);
        }

        // ✏️ DÜZENLE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Slider slider, IFormFile? imageFile)
        {
            if (id != slider.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(slider);
            }

            var existingSlider = await _unitOfWork.Slider.GetAsync(s => s.Id == id, tracked: false);
            if (existingSlider == null) return NotFound();

            // Yeni resim yüklendiyse
            if (imageFile != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string sliderPath = Path.Combine(wwwRootPath, @"images\slider");

                if (!Directory.Exists(sliderPath))
                {
                    Directory.CreateDirectory(sliderPath);
                }

                // Eski resmi sil
                if (!string.IsNullOrEmpty(existingSlider.ImageUrl))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, existingSlider.ImageUrl.TrimStart('\\', '/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Yeni resmi kaydet
                using (var fileStream = new FileStream(Path.Combine(sliderPath, fileName), FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                slider.ImageUrl = @"\images\slider\" + fileName;
            }
            else
            {
                // Resim değiştirilmediyse eski resmi koru
                slider.ImageUrl = existingSlider.ImageUrl;
            }

            _unitOfWork.Slider.Update(slider);
            await _unitOfWork.SaveAsync();
            
            TempData["SuccessMessage"] = "Slider güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // 🗑️ SİL (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var slider = await _unitOfWork.Slider.GetAsync(s => s.Id == id);
            if (slider == null) return NotFound();

            return View(slider);
        }

        // 🗑️ SİL (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slider = await _unitOfWork.Slider.GetAsync(s => s.Id == id);
            if (slider == null) return NotFound();

            // Resmi sil
            if (!string.IsNullOrEmpty(slider.ImageUrl))
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                var imagePath = Path.Combine(wwwRootPath, slider.ImageUrl.TrimStart('\\', '/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _unitOfWork.Slider.RemoveAsync(slider);
            await _unitOfWork.SaveAsync();
            
            TempData["SuccessMessage"] = "Slider silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}