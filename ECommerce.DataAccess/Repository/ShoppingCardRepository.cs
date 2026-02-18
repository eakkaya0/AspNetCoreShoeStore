using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.DataAccess.Data;
using ECommerce.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository 
    {
        private ECommerceDbContext _db;
        public ShoppingCartRepository(ECommerceDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }

        public async Task<IEnumerable<ShoppingCart>> GetUserCartAsync(string userId)
        {
            return await _db.ShoppingCarts
                .Include(sc => sc.Product)
                .Include(sc => sc.ProductVariant)
                .Where(sc => sc.ApplicationUserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShoppingCart>> GetGuestCartAsync(string guestSessionId)
        {
            return await _db.ShoppingCarts
                .Include(sc => sc.Product)
                .Include(sc => sc.ProductVariant)
                .Where(sc => sc.GuestSessionId == guestSessionId)
                .ToListAsync();
        }

        public async Task<ShoppingCart?> GetCartItemAsync(string userId, int productId, int? productVariantId)
        {
            return await _db.ShoppingCarts
                .Include(sc => sc.Product)
                .Include(sc => sc.ProductVariant)
                .FirstOrDefaultAsync(sc => sc.ApplicationUserId == userId 
                                       && sc.ProductId == productId 
                                       && sc.ProductVariantId == productVariantId);
        }

        public async Task<ShoppingCart?> GetGuestCartItemAsync(string guestSessionId, int productId, int? productVariantId)
        {
            return await _db.ShoppingCarts
                .Include(sc => sc.Product)
                .Include(sc => sc.ProductVariant)
                .FirstOrDefaultAsync(sc => sc.GuestSessionId == guestSessionId 
                                       && sc.ProductId == productId 
                                       && sc.ProductVariantId == productVariantId);
        }

        public async Task UpdateQuantityAsync(int cartItemId, int newQuantity)
        {
            var cartItem = await _db.ShoppingCarts.FindAsync(cartItemId);
            if (cartItem != null)
            {
                cartItem.Count = newQuantity;
                _db.ShoppingCarts.Update(cartItem);
            }
        }

        public async Task ClearUserCartAsync(string userId)
        {
            var cartItems = await _db.ShoppingCarts
                .Where(sc => sc.ApplicationUserId == userId)
                .ToListAsync();
            
            _db.ShoppingCarts.RemoveRange(cartItems);
        }

        public async Task ClearGuestCartAsync(string guestSessionId)
        {
            var cartItems = await _db.ShoppingCarts
                .Where(sc => sc.GuestSessionId == guestSessionId)
                .ToListAsync();
            
            _db.ShoppingCarts.RemoveRange(cartItems);
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            return await _db.ShoppingCarts
                .Where(sc => sc.ApplicationUserId == userId)
                .SumAsync(sc => sc.Count);
        }

        public async Task<int> GetGuestCartItemCountAsync(string guestSessionId)
        {
            return await _db.ShoppingCarts
                .Where(sc => sc.GuestSessionId == guestSessionId)
                .SumAsync(sc => sc.Count);
        }
    }
}