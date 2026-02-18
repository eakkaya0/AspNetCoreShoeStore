using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
    public class OrderDetailsVM
    {
        // Müşteri bilgileri
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [Display(Name = "Telefon")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şehir alanı zorunludur.")]
        [Display(Name = "Şehir")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres alanı zorunludur.")]
        [Display(Name = "Adres")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Sipariş Notu")]
        public string? Notes { get; set; }

        // Ödeme bilgileri
        [Display(Name = "Kart Üzerindeki İsim")]
        public string CardHolderName { get; set; } = string.Empty;

        [Display(Name = "Kart Numarası")]
        public string CardNumber { get; set; } = string.Empty;

        [Display(Name = "Son Kullanma Ayı")]
        public string ExpiryMonth { get; set; } = string.Empty;

        [Display(Name = "Son Kullanma Yılı")]
        public string ExpiryYear { get; set; } = string.Empty;

        [Display(Name = "CVV")]
        public string CVV { get; set; } = string.Empty;

        // Sipariş özeti
        public List<CartItemVM> CartItems { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal GrandTotal { get; set; }
        public bool IsGuestUser { get; set; }
    }
}
