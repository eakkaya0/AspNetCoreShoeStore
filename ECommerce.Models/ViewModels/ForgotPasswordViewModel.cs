using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
    /// <summary>
    /// Şifremi Unuttum için ViewModel.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; } = string.Empty;
    }
}
