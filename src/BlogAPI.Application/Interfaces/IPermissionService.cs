using BlogAPI.Application.DTOs;

namespace BlogAPI.Application.Interfaces;

public interface IPermissionService
{
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<PermissionDto?> GetPermissionByIdAsync(Guid id);
    Task<PermissionDto?> GetPermissionByNameAsync(string name);
    Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto);
    Task<PermissionDto?> UpdatePermissionAsync(Guid id, UpdatePermissionDto updatePermissionDto);
    Task<bool> DeletePermissionAsync(Guid id);
}