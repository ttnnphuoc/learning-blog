using System.Linq.Expressions;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly BlogDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(BlogDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(e => !e.IsDeleted).Where(predicate).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
    }

    // Soft Delete Methods
    public virtual async Task SoftDeleteAsync(T entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await UpdateAsync(entity);
    }

    public virtual async Task SoftDeleteAsync(Guid id)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        if (entity != null)
        {
            await SoftDeleteAsync(entity);
        }
    }

    public virtual async Task RestoreAsync(T entity)
    {
        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.UpdatedAt = DateTime.UtcNow;
        await UpdateAsync(entity);
    }

    public virtual async Task RestoreAsync(Guid id)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted);
        if (entity != null)
        {
            await RestoreAsync(entity);
        }
    }

    // Methods to work with trashed items
    public virtual async Task<IEnumerable<T>> GetTrashedAsync()
    {
        return await _dbSet.Where(e => e.IsDeleted).ToListAsync();
    }

    public virtual async Task<T?> GetTrashedByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted);
    }

    // Get all including deleted (for admin purposes)
    public virtual async Task<IEnumerable<T>> GetAllIncludingDeletedAsync()
    {
        return await _dbSet.ToListAsync();
    }
}