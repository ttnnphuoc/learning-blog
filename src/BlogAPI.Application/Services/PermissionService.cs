using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _permissionRepository.GetAllAsync();
        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(Guid id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null) return null;

        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            CreatedAt = permission.CreatedAt,
            UpdatedAt = permission.UpdatedAt
        };
    }

    public async Task<PermissionDto?> GetPermissionByNameAsync(string name)
    {
        var permission = await _permissionRepository.GetByNameAsync(name);
        if (permission == null) return null;

        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            CreatedAt = permission.CreatedAt,
            UpdatedAt = permission.UpdatedAt
        };
    }

    public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto)
    {
        var existingPermission = await _permissionRepository.GetByNameAsync(createPermissionDto.Name);
        if (existingPermission != null)
        {
            throw new InvalidOperationException($"Permission with name '{createPermissionDto.Name}' already exists");
        }

        var permission = new Permission
        {
            Name = createPermissionDto.Name,
            Description = createPermissionDto.Description
        };

        var createdPermission = await _permissionRepository.AddAsync(permission);

        return new PermissionDto
        {
            Id = createdPermission.Id,
            Name = createdPermission.Name,
            Description = createdPermission.Description,
            CreatedAt = createdPermission.CreatedAt,
            UpdatedAt = createdPermission.UpdatedAt
        };
    }

    public async Task<PermissionDto?> UpdatePermissionAsync(Guid id, UpdatePermissionDto updatePermissionDto)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null) return null;

        if (!string.IsNullOrEmpty(updatePermissionDto.Name))
        {
            var existingPermission = await _permissionRepository.GetByNameAsync(updatePermissionDto.Name);
            if (existingPermission != null && existingPermission.Id != id)
            {
                throw new InvalidOperationException($"Permission with name '{updatePermissionDto.Name}' already exists");
            }
            permission.Name = updatePermissionDto.Name;
        }

        if (updatePermissionDto.Description != null)
            permission.Description = updatePermissionDto.Description;

        var updatedPermission = await _permissionRepository.UpdateAsync(permission);

        return new PermissionDto
        {
            Id = updatedPermission.Id,
            Name = updatedPermission.Name,
            Description = updatedPermission.Description,
            CreatedAt = updatedPermission.CreatedAt,
            UpdatedAt = updatedPermission.UpdatedAt
        };
    }

    public async Task<bool> DeletePermissionAsync(Guid id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null) return false;

        if (permission.RolePermissions?.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete permission that is assigned to roles");
        }

        await _permissionRepository.SoftDeleteAsync(permission);
        return true;
    }
}