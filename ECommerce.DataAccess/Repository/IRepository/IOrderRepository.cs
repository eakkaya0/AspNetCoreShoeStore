using ECommerce.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        void Update(Order obj);
    }

    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetOrderItemsAsync(int orderId);
        Task<IEnumerable<int>> GetTopSellingProductIdsAsync(int take);
    }
}
