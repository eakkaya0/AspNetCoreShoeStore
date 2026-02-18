using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
namespace ECommerce.DataAccess.Repository;

  public  interface IRepository<T> where T : class
{
    Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool tracked = true);

    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
    Task AddAsync(T entity);
    Task RemoveAsync(T entity);
    Task RemoveRangeAsync(IEnumerable<T> entities);
}
