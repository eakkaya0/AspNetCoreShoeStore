using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;
using ECommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ECommerceWeb.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        private string GetUserId()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value ?? string.Empty;
        }

        private bool IsGuestUser()
        {
            return string.IsNullOrEmpty(GetUserId());
        }

        private string GetGuestSessionId()
        {
            // Guest kullanıcılar için session ID oluştur
            var guestSessionId = HttpContext.Session.GetString("GuestSessionId");
            if (string.IsNullOrEmpty(guestSessionId))
            {
                guestSessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("GuestSessionId", guestSessionId);
            }
            return guestSessionId;
        }

        /// <summary>
        /// Sepete ürün ekler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz veri." });
            }

            var userId = GetUserId();
            var isGuest = IsGuestUser();
            var guestSessionId = isGuest ? GetGuestSessionId() : string.Empty;

            try
            {
                // Ürün kontrolü
                var product = await _unitOfWork.Product.GetAsync(p => p.Id == model.ProductId);
                if (product == null || !product.IsActive)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı." });
                }

                // Varyant kontrolü (varsa)
                ProductVariant? variant = null;
                if (model.ProductVariantId.HasValue)
                {
                    variant = await _unitOfWork.ProductVariant.GetAsync(v => v.Id == model.ProductVariantId.Value && v.ProductId == model.ProductId);
                    if (variant == null)
                    {
                        return Json(new { success = false, message = "Ürün varyantı bulunamadı." });
                    }

                    if (!variant.IsActive)
                    {
                        return Json(new { success = false, message = "Seçilen varyant satışta değil." });
                    }

                    // Stok kontrolü
                    if (variant.StockQuantity < model.Quantity)
                    {
                        return Json(new { success = false, message = "Yeterli stok yok." });
                    }
                }
                else
                {
                    // Varyant yoksa ana ürün stok kontrolü
                    if (product.StockQuantity < model.Quantity)
                    {
                        return Json(new { success = false, message = "Yeterli stok yok." });
                    }
                }

                // Sepetteki mevcut ürünü kontrol et
                ShoppingCart? existingCart;
                if (isGuest)
                {
                    existingCart = await _unitOfWork.ShoppingCart.GetGuestCartItemAsync(guestSessionId, model.ProductId, model.ProductVariantId);
                }
                else
                {
                    existingCart = await _unitOfWork.ShoppingCart.GetCartItemAsync(userId, model.ProductId, model.ProductVariantId);
                }

                if (existingCart != null)
                {
                    // Mevcut ürünün adedini artır
                    var newQuantity = existingCart.Count + model.Quantity;
                    
                    // Stok kontrolü
                    var availableStock = variant?.StockQuantity ?? product.StockQuantity;
                    if (availableStock < newQuantity)
                    {
                        return Json(new { success = false, message = "Yeterli stok yok." });
                    }

                    existingCart.Count = newQuantity;
                    _unitOfWork.ShoppingCart.Update(existingCart);
                }
                else
                {
                    // Yeni sepet öğesi oluştur
                    var cartItem = new ShoppingCart
                    {
                        ProductId = model.ProductId,
                        ProductVariantId = model.ProductVariantId,
                        Count = model.Quantity,
                        ApplicationUserId = isGuest ? null : userId,
                        GuestSessionId = isGuest ? guestSessionId : null,
                        Price = (double)(variant?.PriceOverride ?? (product.DiscountedPrice ?? product.ListPrice))
                    };

                    await _unitOfWork.ShoppingCart.AddAsync(cartItem);
                }

                await _unitOfWork.SaveAsync();

                var cartCount = isGuest 
                    ? await _unitOfWork.ShoppingCart.GetGuestCartItemCountAsync(guestSessionId)
                    : await _unitOfWork.ShoppingCart.GetCartItemCountAsync(userId);
                    
                return Json(new { success = true, message = "Ürün sepete eklendi.", cartCount });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Bir hata oluştu." });
            }
        }

        /// <summary>
        /// Sepet sayfasını gösterir
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var isGuest = IsGuestUser();
            
            List<ShoppingCart> cartItems;
            
            if (isGuest)
            {
                // Guest kullanıcılar için session bazlı sepet
                var guestSessionId = GetGuestSessionId();
                cartItems = (await _unitOfWork.ShoppingCart.GetGuestCartAsync(guestSessionId)).ToList();
            }
            else
            {
                // Kayıtlı kullanıcılar için user bazlı sepet
                cartItems = (await _unitOfWork.ShoppingCart.GetUserCartAsync(userId)).ToList();
            }
            
            var cartVM = new ShoppingCartVM
            {
                CartItems = cartItems.Select(ci => new CartItemVM
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImageUrl = ci.Product.ImageUrl,
                    ProductVariantId = ci.ProductVariantId,
                    VariantInfo = ci.ProductVariant?.Size,
                    Count = ci.Count,
                    UnitPrice = ci.ProductVariant?.PriceOverride ?? (decimal)(ci.Product.DiscountedPrice ?? ci.Product.ListPrice),
                    StockQuantity = ci.ProductVariant?.StockQuantity ?? ci.Product.StockQuantity
                }).ToList()
            };

            cartVM.CartTotal = cartVM.CartItems.Sum(item => item.TotalPrice);
            cartVM.TotalItems = cartVM.CartItems.Sum(item => item.Count);

            return View(cartVM);
        }

        /// <summary>
        /// Sepet ürünü adedini günceller
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityVM model)
        {
            if (!ModelState.IsValid || model.Quantity < 1)
            {
                return Json(new { success = false, message = "Geçersiz adet." });
            }

            var userId = GetUserId();
            var isGuest = IsGuestUser();
            
            ShoppingCart? cartItem;
            if (isGuest)
            {
                var guestSessionId = GetGuestSessionId();
                cartItem = (await _unitOfWork.ShoppingCart.GetGuestCartAsync(guestSessionId))
                    .FirstOrDefault(ci => ci.Id == model.CartItemId);
            }
            else
            {
                cartItem = (await _unitOfWork.ShoppingCart.GetUserCartAsync(userId))
                    .FirstOrDefault(ci => ci.Id == model.CartItemId);
            }

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            // Stok kontrolü
            var availableStock = cartItem.ProductVariant?.StockQuantity ?? cartItem.Product.StockQuantity;
            if (availableStock < model.Quantity)
            {
                return Json(new { success = false, message = "Yeterli stok yok." });
            }

            cartItem.Count = model.Quantity;
            _unitOfWork.ShoppingCart.Update(cartItem);
            await _unitOfWork.SaveAsync();

            // Güncellenmiş sepet bilgilerini hesapla
            List<ShoppingCart> userCart;
            if (isGuest)
            {
                var guestSessionId = GetGuestSessionId();
                userCart = (await _unitOfWork.ShoppingCart.GetGuestCartAsync(guestSessionId)).ToList();
            }
            else
            {
                userCart = (await _unitOfWork.ShoppingCart.GetUserCartAsync(userId)).ToList();
            }
            
            var cartTotal = userCart.Sum(ci => ci.Count * (decimal)(ci.ProductVariant?.PriceOverride ?? (ci.Product.DiscountedPrice ?? ci.Product.ListPrice)));
            var totalItems = userCart.Sum(ci => ci.Count);

            return Json(new { 
                success = true, 
                message = "Adet güncellendi.",
                itemTotal = model.Quantity * (decimal)(cartItem.ProductVariant?.PriceOverride ?? (cartItem.Product.DiscountedPrice ?? cartItem.Product.ListPrice)),
                cartTotal = cartTotal,
                totalItems = totalItems
            });
        }

        /// <summary>
        /// Sepetten ürün siler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartRequest request)
        {
            var userId = GetUserId();
            var isGuest = IsGuestUser();
            
            // Eğer CartItemId 0 ise ProductId ile bul
            ShoppingCart? cartItem;
            if (request.CartItemId == 0 && request.ProductId > 0)
            {
                if (isGuest)
                {
                    var guestSessionId = GetGuestSessionId();
                    cartItem = (await _unitOfWork.ShoppingCart.GetGuestCartAsync(guestSessionId))
                        .FirstOrDefault(ci => ci.ProductId == request.ProductId);
                }
                else
                {
                    cartItem = (await _unitOfWork.ShoppingCart.GetUserCartAsync(userId))
                        .FirstOrDefault(ci => ci.ProductId == request.ProductId);
                }
            }
            else
            {
                if (isGuest)
                {
                    var guestSessionId = GetGuestSessionId();
                    cartItem = (await _unitOfWork.ShoppingCart.GetGuestCartAsync(guestSessionId))
                        .FirstOrDefault(ci => ci.Id == request.CartItemId);
                }
                else
                {
                    cartItem = (await _unitOfWork.ShoppingCart.GetUserCartAsync(userId))
                        .FirstOrDefault(ci => ci.Id == request.CartItemId);
                }
            }

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            await _unitOfWork.ShoppingCart.RemoveAsync(cartItem);
            await _unitOfWork.SaveAsync();

            // Güncellenmiş sepet bilgilerini hesapla
            List<ShoppingCart> userCart;
            if (isGuest)
            {
                var guestSessionId = GetGuestSessionId();
                userCart = (await _unitOfWork.ShoppingCart.GetGuestCartAsync(guestSessionId)).ToList();
            }
            else
            {
                userCart = (await _unitOfWork.ShoppingCart.GetUserCartAsync(userId)).ToList();
            }
            
            var cartTotal = userCart.Sum(ci => ci.Count * (decimal)(ci.ProductVariant?.PriceOverride ?? (ci.Product.DiscountedPrice ?? ci.Product.ListPrice)));
            var totalItems = userCart.Sum(ci => ci.Count);

            return Json(new { 
                success = true, 
                message = "Ürün sepetten kaldırıldı.",
                cartTotal = cartTotal,
                totalItems = totalItems
            });
        }

        /// <summary>
        /// Sepeti tamamen boşaltır
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var isGuest = IsGuestUser();
            
            if (isGuest)
            {
                var guestSessionId = GetGuestSessionId();
                await _unitOfWork.ShoppingCart.ClearGuestCartAsync(guestSessionId);
            }
            else
            {
                await _unitOfWork.ShoppingCart.ClearUserCartAsync(userId);
            }
            
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "Sepet boşaltıldı." });
        }

        /// <summary>
        /// Header'da gösterilecek sepet ürün sayısı için
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = GetUserId();
            var isGuest = IsGuestUser();
            
            if (isGuest)
            {
                var guestSessionId = GetGuestSessionId();
                var count = await _unitOfWork.ShoppingCart.GetGuestCartItemCountAsync(guestSessionId);
                return Json(count);
            }
            else
            {
                var count = await _unitOfWork.ShoppingCart.GetCartItemCountAsync(userId);
                return Json(count);
            }
        }
    }

    /// <summary>
    /// Adet güncelleme için ViewModel
    /// </summary>
    public class UpdateQuantityVM
    {
        public int CartItemId { get; set; }
        [Range(1, 1000)]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Sepetten ürün silme için ViewModel
    /// </summary>
    public class RemoveFromCartRequest
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
    }
}
