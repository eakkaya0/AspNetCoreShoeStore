using System;
using ECommerce.Models.Models;

namespace ECommerce.DataAccess.Repository.IRepository;

public interface ISliderRepository : IRepository<Slider>
{
    void Update(Slider slider);
}