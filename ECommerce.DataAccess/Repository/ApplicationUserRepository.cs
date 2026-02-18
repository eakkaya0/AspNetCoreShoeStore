using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;
using ECommerce.Models.Identity;

namespace ECommerce.DataAccess.Repository;

public class ApplicationUserRepository 
    : Repository<ApplicationUser> , IApplicationUserRepository
{
    private readonly ECommerceDbContext _context;

    public ApplicationUserRepository(ECommerceDbContext context) 
        : base(context) 
    {
        _context = context;
    }

    
   
}
