using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;

namespace ECommerce.DataAccess.Repository
{
    public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository
    {
        private readonly ECommerceDbContext _db;
        
        public ProductVariantRepository(ECommerceDbContext context) : base(context)
        {
            _db = context;
        }

        public void Update(ProductVariant obj)
        {
            _db.ProductVariants.Update(obj);
        }
    }
}
