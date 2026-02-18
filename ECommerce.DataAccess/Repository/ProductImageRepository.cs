using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;

namespace ECommerce.DataAccess.Repository;

public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(ECommerceDbContext context) : base(context)
    {
    }
}
