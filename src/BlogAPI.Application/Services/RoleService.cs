using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    public RoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        });
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return null;

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string name)
    {
        var role = await _roleRepository.GetByNameAsync(name);
        if (role == null) return null;

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto)
    {
        var existingRole = await _roleRepository.GetByNameAsync(createRoleDto.Name);
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role with name '{createRoleDto.Name}' already exists");
        }

        var role = new Role
        {
            Name = createRoleDto.Name,
            Description = createRoleDto.Description
        };

        var createdRole = await _roleRepository.AddAsync(role);

        return new RoleDto
        {
            Id = createdRole.Id,
            Name = createdRole.Name,
            Description = createdRole.Description,
            CreatedAt = createdRole.CreatedAt,
            UpdatedAt = createdRole.UpdatedAt
        };
    }

    public async Task<RoleDto?> UpdateRoleAsync(Guid id, UpdateRoleDto updateRoleDto)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return null;

        if (!string.IsNullOrEmpty(updateRoleDto.Name))
        {
            var existingRole = await _roleRepository.GetByNameAsync(updateRoleDto.Name);
            if (existingRole != null && existingRole.Id != id)
            {
                throw new InvalidOperationException($"Role with name '{updateRoleDto.Name}' already exists");
            }
            role.Name = updateRoleDto.Name;
        }

        if (updateRoleDto.Description != null)
            role.Description = updateRoleDto.Description;

        var updatedRole = await _roleRepository.UpdateAsync(role);

        return new RoleDto
        {
            Id = updatedRole.Id,
            Name = updatedRole.Name,
            Description = updatedRole.Description,
            CreatedAt = updatedRole.CreatedAt,
            UpdatedAt = updatedRole.UpdatedAt
        };
    }

    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return false;

        if (role.UserRoles?.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete role that has associated users");
        }

        await _roleRepository.SoftDeleteAsync(role);
        return true;
    }

    public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null) return false;

        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        if (permission == null) return false;

        await _roleRepository.AddPermissionToRoleAsync(roleId, permissionId);
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null) return false;

        await _roleRepository.RemovePermissionFromRoleAsync(roleId, permissionId);
        return true;
    }

    public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(Guid roleId)
    {
        var role = await _roleRepository.GetWithPermissionsAsync(roleId);
        if (role?.Permissions == null) return new List<PermissionDto>();

        return role.Permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });
    }
}