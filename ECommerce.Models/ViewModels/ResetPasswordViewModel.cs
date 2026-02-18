using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
    /// <summary>
    /// Şifre Sıfırlama için ViewModel.
    /// </summary>
    public class ResetPasswordViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre minimum 6 karakter olmalıdır.")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
