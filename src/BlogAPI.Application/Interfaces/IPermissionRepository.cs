using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name);
    Task<IEnumerable<Permission>> GetByResourceAsync(string resource);
    Task<IEnumerable<Permission>> GetByCategoryAsync(string category);
}