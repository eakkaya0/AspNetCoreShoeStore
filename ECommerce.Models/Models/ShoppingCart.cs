using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ECommerce.Models.Identity;

namespace ECommerce.Models.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }
        
        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        public int Count { get; set; }

        // Kayıtlı kullanıcı için
        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        // Guest kullanıcı için
        public string? GuestSessionId { get; set; }

        [NotMapped]
        public double Price { get; set; }

        // Varyant (beden) desteği
        public int? ProductVariantId { get; set; }
        [ForeignKey("ProductVariantId")]
        [ValidateNever]
        public ProductVariant? ProductVariant { get; set; }
    }
}

