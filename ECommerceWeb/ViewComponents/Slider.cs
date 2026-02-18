using Microsoft.AspNetCore.Mvc;
using ECommerce.DataAccess.Data;

namespace ECommerceWeb.ViewComponents;

public class Slider : ViewComponent
{
    private readonly ECommerceDbContext _context;

    public Slider(ECommerceDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var sliders = _context.Sliders
                              .Where(x => x.IsActive)
                              .OrderBy(x => x.DisplayOrder)
                              .ToList();

        return View(sliders);
    }
}
