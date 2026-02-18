using ECommerce.DataAccess.Repository;
using ECommerce.DataAccess.Repository.IRepository;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.DataAccess;

/// <summary>
/// Extension methods for IServiceCollection to register repository and UnitOfWork services.
/// This follows the SOLID principle - Dependency Inversion Principle.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all repository interfaces and implementations along with the Unit of Work pattern.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        // Register individual repositories
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<ISliderRepository, SliderRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();

        // Register Unit of Work - depends on the repositories above
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}