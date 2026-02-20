using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;
using ECommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceWeb.Controllers
{
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
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
                
                // Cookie'e de kaydet (backup)
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddHours(24),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = false
                };
                Response.Cookies.Append("GuestSessionId", guestSessionId, cookieOptions);
                
                Console.WriteLine($"Created new guest session ID: {guestSessionId}");
            }
            else
            {
                // Session'da yoksa cookie'den al
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("GuestSessionId")))
                {
                    var cookieSessionId = Request.Cookies["GuestSessionId"];
                    if (!string.IsNullOrEmpty(cookieSessionId))
                    {
                        HttpContext.Session.SetString("GuestSessionId", cookieSessionId);
                        guestSessionId = cookieSessionId;
                    }
                }
                Console.WriteLine($"Using existing guest session ID: {guestSessionId}");
            }
            return guestSessionId;
        }

        /// <summary>
        /// Sipariş detayları sayfası
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> OrderDetails()
        {
            var userId = GetUserId();
            var isGuest = IsGuestUser();
            
            Console.WriteLine($"OrderDetails - IsGuest: {isGuest}, UserId: {userId}");
            
            List<ShoppingCart> cartItems;
            
            if (isGuest)
            {
                // Guest kullanıcılar için session bazlı sepet
                var guestSessionId = GetGuestSessionId();
                Console.WriteLine($"OrderDetails - Getting guest cart for session: {guestSessionId}");
                cartItems = (await _unitOfWork.ShoppingCart.GetGuestCartAsync(guestSessionId)).ToList();
                Console.WriteLine($"OrderDetails - Guest cart items count: {cartItems.Count}");
            }
            else
            {
                // Kayıtlı kullanıcılar için user bazlı sepet
                cartItems = (await _unitOfWork.ShoppingCart.GetUserCartAsync(userId)).ToList();
                Console.WriteLine($"OrderDetails - User cart items count: {cartItems.Count}");
            }

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "ShoppingCart");
            }

            var orderDetailsVM = new OrderDetailsVM
            {
                IsGuestUser = isGuest,
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

            orderDetailsVM.Subtotal = orderDetailsVM.CartItems.Sum(item => item.TotalPrice);
            orderDetailsVM.ShippingCost = orderDetailsVM.Subtotal > 500 ? 0 : 50; // 500 TL üzeri ücretsiz kargo
            orderDetailsVM.GrandTotal = orderDetailsVM.Subtotal + orderDetailsVM.ShippingCost;

            return View(orderDetailsVM);
        }

        /// <summary>
        /// Siparişi tamamlama
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder([FromForm] OrderDetailsVM model)
        {
            Console.WriteLine($"CompleteOrder - Starting order completion");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"CompleteOrder - Model invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                // Sepet verilerini yeniden yükle
                await LoadCartData(model);
                return View("OrderDetails", model);
            }

            var userId = GetUserId();
            var isGuest = IsGuestUser();
            
            Console.WriteLine($"CompleteOrder - IsGuest: {isGuest}, UserId: {userId}");
            
            List<ShoppingCart> cartItems;
            
            if (isGuest)
            {
                var guestSessionId = GetGuestSessionId();
                Console.WriteLine($"CompleteOrder - Getting guest cart for session: {guestSessionId}");
                cartItems = (await _unitOfWork.ShoppingCart.GetGuestCartAsync(guestSessionId)).ToList();
                Console.WriteLine($"CompleteOrder - Guest cart items count: {cartItems.Count}");
            }
            else
            {
                cartItems = (await _unitOfWork.ShoppingCart.GetUserCartAsync(userId)).ToList();
                Console.WriteLine($"CompleteOrder - User cart items count: {cartItems.Count}");
            }

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "ShoppingCart");
            }

            // Stok kontrolü
            foreach (var cartItem in cartItems)
            {
                var availableStock = cartItem.ProductVariant?.StockQuantity ?? cartItem.Product.StockQuantity;
                if (availableStock < cartItem.Count)
                {
                    ModelState.AddModelError("", $"{cartItem.Product.Name} ürünü için yeterli stok yok. Mevcut stok: {availableStock}");
                    await LoadCartData(model);
                    return View("OrderDetails", model);
                }
            }

            try
            {
                // Sipariş oluştur
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    TotalAmount = model.Subtotal,
                    ShippingCost = model.ShippingCost
                };

                // Guest kullanıcı bilgileri
                if (isGuest)
                {
                    order.GuestEmail = model.Email;
                    order.GuestFirstName = model.FirstName;
                    order.GuestLastName = model.LastName;
                    order.GuestPhone = model.Phone;
                    order.GuestCity = model.City;
                    order.GuestAddress = model.Address;
                    order.GuestNotes = model.Notes;
                }
                else
                {
                    order.ApplicationUserId = userId;

                    // Kayıtlı kullanıcı siparişinde de teslimat bilgilerini siparişe yaz
                    // (Admin panelinde sipariş bazlı adres/telefon görünmesi için)
                    order.GuestEmail = model.Email;
                    order.GuestFirstName = model.FirstName;
                    order.GuestLastName = model.LastName;
                    order.GuestPhone = model.Phone;
                    order.GuestCity = model.City;
                    order.GuestAddress = model.Address;
                    order.GuestNotes = model.Notes;
                }

                await _unitOfWork.Order.AddAsync(order);
                await _unitOfWork.SaveAsync();

                // Sipariş öğelerini oluştur
                var affectedProductIds = new HashSet<int>();
                foreach (var cartItem in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        ProductVariantId = cartItem.ProductVariantId,
                        Quantity = cartItem.Count,
                        UnitPrice = cartItem.ProductVariant?.PriceOverride ?? (decimal)(cartItem.Product.DiscountedPrice ?? cartItem.Product.ListPrice)
                    };

                    await _unitOfWork.OrderItem.AddAsync(orderItem);

                    // Stoğu düş
                    if (cartItem.ProductVariant != null)
                    {
                        cartItem.ProductVariant.StockQuantity -= cartItem.Count;
                        _unitOfWork.ProductVariant.Update(cartItem.ProductVariant);

                        affectedProductIds.Add(cartItem.ProductId);
                    }
                    else
                    {
                        cartItem.Product.StockQuantity -= cartItem.Count;
                        _unitOfWork.Product.Update(cartItem.Product);
                    }
                }

                // Varyantlı ürünlerde ürünün toplam stok bilgisini güncel tut
                // (Admin panelinde ve diğer yerlerde Product.StockQuantity doğru görünsün)
                foreach (var productId in affectedProductIds)
                {
                    var productToUpdate = await _unitOfWork.Product.GetAsync(p => p.Id == productId);
                    if (productToUpdate == null)
                        continue;

                    var activeVariants = (await _unitOfWork.ProductVariant.GetAllAsync(v => v.ProductId == productId && v.IsActive)).ToList();
                    if (activeVariants.Any())
                    {
                        productToUpdate.StockQuantity = activeVariants.Sum(v => v.StockQuantity);
                        _unitOfWork.Product.Update(productToUpdate);
                    }
                }

                // Sepeti temizle
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

                // Ödeme simülasyonu (iyzico test için)
                order.PaymentReference = $"TEST_{DateTime.Now:yyyyMMddHHmmss}";
                order.PaymentDate = DateTime.Now;
                order.IsPaid = true;
                order.Status = OrderStatus.Confirmed;

                _unitOfWork.Order.Update(order);
                await _unitOfWork.SaveAsync();

                return RedirectToAction("OrderSuccess", new { orderId = order.Id });
            }
            catch 
            {
                ModelState.AddModelError("", "Sipariş oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                await LoadCartData(model);
                return View("OrderDetails", model);
            }
        }

        /// <summary>
        /// Sipariş başarılı sayfası
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int orderId)
        {
            var order = await _unitOfWork.Order.GetOrderWithDetailsAsync(orderId);
            
            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }

        private async Task LoadCartData(OrderDetailsVM model)
        {
            var userId = GetUserId();
            var isGuest = IsGuestUser();
            
            List<ShoppingCart> cartItems;
            
            if (isGuest)
            {
                var guestSessionId = GetGuestSessionId();
                cartItems = (await _unitOfWork.ShoppingCart.GetGuestCartAsync(guestSessionId)).ToList();
            }
            else
            {
                cartItems = (await _unitOfWork.ShoppingCart.GetUserCartAsync(userId)).ToList();
            }

            model.CartItems = cartItems.Select(ci => new CartItemVM
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
            }).ToList();

            model.Subtotal = model.CartItems.Sum(item => item.TotalPrice);
            model.ShippingCost = model.Subtotal > 500 ? 0 : 50;
            model.GrandTotal = model.Subtotal + model.ShippingCost;
        }

        private string GenerateOrderNumber()
        {
            return $"ORD{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }
    }
}
