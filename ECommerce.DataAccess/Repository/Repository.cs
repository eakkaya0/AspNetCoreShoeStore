using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ECommerce.DataAccess.Data;

namespace ECommerce.DataAccess.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ECommerceDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ECommerceDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.ToListAsync();
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool tracked = true)
    {
        IQueryable<T> query = _context.Set<T>();

        if (!tracked)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(filter);
    }


    public Task RemoveAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }
}
