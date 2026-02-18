using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.Models.Identity;
using ECommerce.Models.ViewModels;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;

namespace ECommerceWeb.Controllers
{
    /// <summary>
    /// Admin paneli controller. Sistem yönetimi işlemleri burada yapılır.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Admin paneli ana sayfası - tüm kullanıcıları ve istatistikler listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userListViewModels = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userListViewModels.Add(new UserListViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                });
            }

            // İstatistik verileri
            var totalUsers = users.Count;
            var totalOrders = (await _unitOfWork.Order.GetAllOrdersAsync()).Count();
            var pendingOrders = (await _unitOfWork.Order.GetAllOrdersAsync()).Count(o => o.Status == OrderStatus.Pending);
            var totalRevenue = (await _unitOfWork.Order.GetAllOrdersAsync()).Where(o => o.IsPaid).Sum(o => o.GrandTotal);

            var adminDashboardVM = new AdminDashboardVM
            {
                TotalUsers = totalUsers,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                TotalRevenue = totalRevenue,
                RecentUsers = users.OrderByDescending(u => u.CreatedAt).Take(5).ToList(),
                UserListViewModels = userListViewModels
            };

            return View(adminDashboardVM);
        }

        /// <summary>
        /// Kullanıcı detayını gösterir ve rol atama işlemlerini gerçekleştirir.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UserDetail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var roleSelections = allRoles.Select(role => new RoleSelectionViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name ?? "",
                IsSelected = userRoles.Contains(role.Name ?? "")
            }).ToList();

            var viewModel = new UserDetailViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                Roles = roleSelections
            };

            return View(viewModel);
        }

        /// <summary>
        /// Kullanıcıya rol atar veya kaldırır.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoles(string userId, List<string> selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            var userRoles = await _userManager.GetRolesAsync(user);

            // Seçilmeyen rolleri kaldır
            foreach (var role in userRoles)
            {
                if (!selectedRoles.Contains(role))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
            }

            // Seçilen yeni rolleri ekle
            foreach (var role in selectedRoles)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            TempData["Success"] = "Kullanıcı rolleri başarıyla güncellendi.";
            return RedirectToAction(nameof(UserDetail), new { userId });
        }

        /// <summary>
        /// Kullanıcının şifresini sıfırlar (admin işlemi).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["Error"] = "Yeni şifre boş olamaz.";
                return RedirectToAction(nameof(UserDetail), new { userId });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Kullanıcı şifresi başarıyla sıfırlandı.";
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                TempData["Error"] = $"Şifre sıfırlanırken hata oluştu: {errors}";
            }

            return RedirectToAction(nameof(UserDetail), new { userId });
        }

        /// <summary>
        /// Kullanıcının e-mail doğrulama durumunu verifies.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmUserEmail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Kullanıcı e-maili onaylandı.";
                }
                else
                {
                    TempData["Error"] = "E-mail onaylanırken hata oluştu.";
                }
            }

            return RedirectToAction(nameof(UserDetail), new { userId });
        }

        /// <summary>
        /// Kullanıcıyı siler (geri alınamaz işlem).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            // Admin kullanıcısını koruma
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(ApplicationRoleNames.Admin))
            {
                TempData["Error"] = "Admin kullanıcılar silinemez.";
                return RedirectToAction(nameof(UserDetail), new { userId });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Kullanıcı başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            TempData["Error"] = $"Kullanıcı silinirken hata oluştu: {errors}";
            return RedirectToAction(nameof(UserDetail), new { userId });
        }
    }
}
