using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Infrastructure.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(BlogDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .Where(r => !r.IsDeleted)
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Role?> GetWithPermissionsAsync(Guid id)
    {
        return await _context.Roles
            .Where(r => !r.IsDeleted)
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Role>> GetSystemRolesAsync()
    {
        return await _context.Roles
            .Where(r => !r.IsDeleted && r.IsSystemRole)
            .Include(r => r.Permissions)
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync()
    {
        return await _context.Roles
            .Where(r => !r.IsDeleted && !r.IsSystemRole)
            .Include(r => r.Permissions)
            .ToListAsync();
    }

    public async Task AddPermissionToRoleAsync(Guid roleId, Guid permissionId)
    {
        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();
    }

    public async Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();
        }
    }
}