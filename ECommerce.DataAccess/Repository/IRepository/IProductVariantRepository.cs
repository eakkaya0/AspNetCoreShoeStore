using ECommerce.Models.Models;

namespace ECommerce.DataAccess.Repository.IRepository;

public interface IProductVariantRepository : IRepository<ProductVariant>
{
    void Update(ProductVariant obj);
}
