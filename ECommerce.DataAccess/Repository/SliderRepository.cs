using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;

namespace ECommerce.DataAccess.Repository;

public class SliderRepository : Repository<Slider>, ISliderRepository
{
    private readonly ECommerceDbContext _context;

    public SliderRepository(ECommerceDbContext context) : base(context)
    {
        _context = context;
    }

    public void Update(Slider slider)
    {
        _context.Sliders.Update(slider);
    }
}