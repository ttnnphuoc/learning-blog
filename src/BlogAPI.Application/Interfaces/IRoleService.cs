using BlogAPI.Application.DTOs;

namespace BlogAPI.Application.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(Guid id);
    Task<RoleDto?> GetRoleByNameAsync(string name);
    Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto);
    Task<RoleDto?> UpdateRoleAsync(Guid id, UpdateRoleDto updateRoleDto);
    Task<bool> DeleteRoleAsync(Guid id);
    Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId);
    Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId);
    Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(Guid roleId);
}