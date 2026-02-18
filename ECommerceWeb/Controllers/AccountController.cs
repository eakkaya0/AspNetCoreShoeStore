using System.Threading.Tasks;
using ECommerce.Models.Identity;
using ECommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;
using System.Security.Claims;

namespace ECommerceWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender<ApplicationUser> _emailSender;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender<ApplicationUser> emailSender,
            IUnitOfWork unitOfWork)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _emailSender   = emailSender;
            _unitOfWork = unitOfWork;
        }

        private string GetUserId()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value ?? string.Empty;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName      = model.Email,
                Email         = model.Email,
                FirstName     = model.FirstName,
                LastName      = model.LastName,
                EmailConfirmed = true  // Development'da otomatik doğrula
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, ApplicationRoleNames.Customer);

            TempData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return Redirect(returnUrl ?? Url.Action("Index", "Home")!);

            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "Hesap kilitlendi. Lütfen daha sonra tekrar deneyin.");
            else
                ModelState.AddModelError(string.Empty, "Geçersiz giriş.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Sadece kayıtlı kullanıcılar şifre sıfırlayabilir
                ModelState.AddModelError(string.Empty, "Bu e-posta adresi kayıtlı değil. Lütfen kayıt olun.");
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(nameof(ResetPassword), "Account",
                new { userId = user.Id, token }, Request.Scheme) ?? "";

            await _emailSender.SendPasswordResetLinkAsync(user, user.Email!, callbackUrl);

            // Development ortamında hızlı test için bağlantıyı göster
            TempData["ResetLink"] = callbackUrl;
            TempData["Success"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
            return RedirectToAction(nameof(PasswordResetSent));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult PasswordResetSent() => View();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Geçersiz şifre sıfırlama bağlantısı.");

            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                // E-posta onayını etkinleştir (email confirmation required için)
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }

                TempData["Success"] = "Şifreniz başarıyla sıfırlanmıştır. Giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        /// <summary>
        /// Kullanıcının sipariş geçmişini gösterir
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Orders()
        {
            var userId = GetUserId();
            var orders = await _unitOfWork.Order.GetUserOrdersAsync(userId);
            
            var orderVMs = orders.Select(o => new OrderSummaryVM
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Status = o.Status,
                StatusText = GetStatusText(o.Status),
                StatusColor = GetStatusColor(o.Status),
                TotalAmount = o.TotalAmount,
                ShippingCost = o.ShippingCost,
                GrandTotal = o.GrandTotal,
                IsPaid = o.IsPaid,
                OrderItems = o.OrderItems.Select(oi => new OrderItemVM
                {
                    Id = oi.Id,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    VariantInfo = oi.ProductVariant?.Size,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            }).ToList();

            return View(orderVMs);
        }

        /// <summary>
        /// Sipariş detaylarını gösterir
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetUserId();
            var order = await _unitOfWork.Order.GetOrderWithDetailsAsync(id);
            
            if (order == null || order.ApplicationUserId != userId)
            {
                return RedirectToAction("Orders");
            }

            return View(order);
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Beklemede",
                OrderStatus.Confirmed => "Onaylandı",
                OrderStatus.Preparing => "Hazırlanıyor",
                OrderStatus.Shipped => "Kargoda",
                OrderStatus.Delivered => "Teslim Edildi",
                OrderStatus.Cancelled => "İptal Edildi",
                OrderStatus.Refunded => "İade Edildi",
                _ => "Bilinmeyen"
            };
        }

        private string GetStatusColor(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "warning",
                OrderStatus.Confirmed => "info",
                OrderStatus.Preparing => "primary",
                OrderStatus.Shipped => "success",
                OrderStatus.Delivered => "success",
                OrderStatus.Cancelled => "danger",
                OrderStatus.Refunded => "secondary",
                _ => "secondary"
            };
        }
    }
}

