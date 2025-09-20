using System.Linq.Expressions;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(Guid id);
    
    // Soft Delete Methods
    Task SoftDeleteAsync(T entity);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(T entity);
    Task RestoreAsync(Guid id);
    Task<IEnumerable<T>> GetTrashedAsync();
    Task<T?> GetTrashedByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllIncludingDeletedAsync();
}