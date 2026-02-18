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
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private ECommerceDbContext _db;
        public OrderRepository(ECommerceDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .Where(o => o.ApplicationUserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public void Update(Order obj)
        {
            _db.Orders.Update(obj);
        }
    }

    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        private ECommerceDbContext _db;
        public OrderItemRepository(ECommerceDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsAsync(int orderId)
        {
            return await _db.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.ProductVariant)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetTopSellingProductIdsAsync(int take)
        {
            return await _db.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalQty)
                .ThenByDescending(x => x.ProductId)
                .Take(take)
                .Select(x => x.ProductId)
                .ToListAsync();
        }
    }
}
