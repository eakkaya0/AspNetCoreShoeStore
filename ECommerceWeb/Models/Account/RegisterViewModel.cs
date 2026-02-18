using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.Models.Account
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required, Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifre (tekrar)")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

