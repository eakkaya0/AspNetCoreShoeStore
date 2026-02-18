using ECommerce.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart obj);
        
        /// <summary>
        /// Kullanıcının sepetindeki tüm ürünleri getirir
        /// </summary>
        Task<IEnumerable<ShoppingCart>> GetUserCartAsync(string userId);
        
        /// <summary>
        /// Guest kullanıcının sepetindeki tüm ürünleri getirir
        /// </summary>
        Task<IEnumerable<ShoppingCart>> GetGuestCartAsync(string guestSessionId);
        
        /// <summary>
        /// Kullanıcının sepetindeki belirli bir ürünü getirir
        /// </summary>
        Task<ShoppingCart?> GetCartItemAsync(string userId, int productId, int? productVariantId);
        
        /// <summary>
        /// Guest kullanıcının sepetindeki belirli bir ürünü getirir
        /// </summary>
        Task<ShoppingCart?> GetGuestCartItemAsync(string guestSessionId, int productId, int? productVariantId);
        
        /// <summary>
        /// Kullanıcının sepetindeki ürün adedini günceller
        /// </summary>
        Task UpdateQuantityAsync(int cartItemId, int newQuantity);
        
        /// <summary>
        /// Kullanıcının sepetindeki tüm ürünlerini siler
        /// </summary>
        Task ClearUserCartAsync(string userId);
        
        /// <summary>
        /// Guest kullanıcının sepetindeki tüm ürünlerini siler
        /// </summary>
        Task ClearGuestCartAsync(string guestSessionId);
        
        /// <summary>
        /// Kullanıcının sepetindeki toplam ürün adedini getirir
        /// </summary>
        Task<int> GetCartItemCountAsync(string userId);
        
        /// <summary>
        /// Guest kullanıcının sepetindeki toplam ürün adedini getirir
        /// </summary>
        Task<int> GetGuestCartItemCountAsync(string guestSessionId);
    }
}