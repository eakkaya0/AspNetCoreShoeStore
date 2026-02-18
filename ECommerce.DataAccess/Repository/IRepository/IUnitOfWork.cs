using System;

namespace ECommerce.DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    ICategoryRepository Category { get; }
    IProductRepository Product { get; }
    IProductVariantRepository ProductVariant { get; }
    IProductImageRepository ProductImage { get; }
    ISliderRepository Slider { get; }
    IShoppingCartRepository ShoppingCart { get; }
    IApplicationUserRepository ApplicationUser { get; }
    IOrderRepository Order { get; }
    IOrderItemRepository OrderItem { get; }

    Task SaveAsync();
}
