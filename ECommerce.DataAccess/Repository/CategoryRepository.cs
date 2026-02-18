using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;

namespace ECommerce.DataAccess.Repository;

public class CategoryRepository 
    : Repository<Category>, ICategoryRepository
{
    private readonly ECommerceDbContext _context;

    public CategoryRepository(ECommerceDbContext context) 
        : base(context)
    {
        _context = context;
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

   
}
