using System;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.DataAccess.Data;

namespace ECommerce.DataAccess.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    public ICategoryRepository Category { get; private set; }
    public IProductRepository Product { get; private set; }
    public IProductVariantRepository ProductVariant { get; private set; }
    public IProductImageRepository ProductImage { get; private set; }
    public ISliderRepository Slider { get; private set; }
    public IShoppingCartRepository ShoppingCart { get; private set; }
    public IApplicationUserRepository ApplicationUser { get; private set; }
    public IOrderRepository Order { get; private set; }
    public IOrderItemRepository OrderItem { get; private set; }

    public UnitOfWork(
        ECommerceDbContext context,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IProductVariantRepository productVariantRepository,
        IProductImageRepository productImageRepository,
        IShoppingCartRepository shoppingCartRepository,
        IApplicationUserRepository applicationUserRepository,
        ISliderRepository sliderRepository,
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository)
    {
        _context = context;
        Category = categoryRepository;
        Product = productRepository;
        ShoppingCart = shoppingCartRepository;
        ProductVariant = productVariantRepository;
        ProductImage = productImageRepository;
        Slider = sliderRepository;
        ApplicationUser = applicationUserRepository;
        Order = orderRepository;
        OrderItem = orderItemRepository;
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
