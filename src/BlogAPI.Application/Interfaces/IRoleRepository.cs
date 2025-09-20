using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> GetWithPermissionsAsync(Guid id);
    Task<IEnumerable<Role>> GetSystemRolesAsync();
    Task<IEnumerable<Role>> GetUserRolesAsync();
    Task AddPermissionToRoleAsync(Guid roleId, Guid permissionId);
    Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId);
}