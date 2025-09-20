using Microsoft.AspNetCore.Mvc;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleRepository roleRepository, IPermissionRepository permissionRepository, ILogger<RolesController> logger)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        try
        {
            var roles = await _roleRepository.GetAllAsync();
            var roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Permissions = r.Permissions?.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Resource = p.Resource,
                    Action = p.Action,
                    Description = p.Description,
                    Category = p.Category,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList() ?? new List<PermissionDto>()
            });

            return Ok(roleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, "An error occurred while retrieving roles");
        }
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRole(Guid id)
    {
        try
        {
            var role = await _roleRepository.GetWithPermissionsAsync(id);
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found");
            }

            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                Permissions = role.Permissions?.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Resource = p.Resource,
                    Action = p.Action,
                    Description = p.Description,
                    Category = p.Category,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList() ?? new List<PermissionDto>()
            };

            return Ok(roleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleId}", id);
            return StatusCode(500, "An error occurred while retrieving the role");
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto createRoleDto)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(createRoleDto.Name))
            {
                return BadRequest("Role name is required");
            }

            // Check if role already exists
            var existingRole = await _roleRepository.GetByNameAsync(createRoleDto.Name);
            if (existingRole != null)
            {
                return Conflict($"A role with name '{createRoleDto.Name}' already exists");
            }

            var role = new Role
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                IsSystemRole = createRoleDto.IsSystemRole
            };

            var createdRole = await _roleRepository.AddAsync(role);

            // Add permissions if provided
            if (createRoleDto.PermissionIds?.Count > 0)
            {
                foreach (var permissionId in createRoleDto.PermissionIds)
                {
                    var permission = await _permissionRepository.GetByIdAsync(permissionId);
                    if (permission != null)
                    {
                        await _roleRepository.AddPermissionToRoleAsync(createdRole.Id, permissionId);
                    }
                }
            }

            // Get the role with permissions for response
            var roleWithPermissions = await _roleRepository.GetWithPermissionsAsync(createdRole.Id);

            var roleDto = new RoleDto
            {
                Id = roleWithPermissions!.Id,
                Name = roleWithPermissions.Name,
                Description = roleWithPermissions.Description,
                IsSystemRole = roleWithPermissions.IsSystemRole,
                CreatedAt = roleWithPermissions.CreatedAt,
                UpdatedAt = roleWithPermissions.UpdatedAt,
                Permissions = roleWithPermissions.Permissions?.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Resource = p.Resource,
                    Action = p.Action,
                    Description = p.Description,
                    Category = p.Category,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList() ?? new List<PermissionDto>()
            };

            return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, roleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, "An error occurred while creating the role");
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole(Guid id, UpdateRoleDto updateRoleDto)
    {
        try
        {
            var role = await _roleRepository.GetWithPermissionsAsync(id);
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found");
            }

            // Prevent modification of system roles
            if (role.IsSystemRole)
            {
                return BadRequest("System roles cannot be modified");
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(updateRoleDto.Name))
            {
                // Check if new name already exists (and it's not the current role)
                var existingRole = await _roleRepository.GetByNameAsync(updateRoleDto.Name);
                if (existingRole != null && existingRole.Id != id)
                {
                    return Conflict($"A role with name '{updateRoleDto.Name}' already exists");
                }
                role.Name = updateRoleDto.Name;
            }

            if (updateRoleDto.Description != null)
            {
                role.Description = updateRoleDto.Description;
            }

            await _roleRepository.UpdateAsync(role);

            // Update permissions if provided
            if (updateRoleDto.PermissionIds != null)
            {
                // Remove existing permissions
                if (role.Permissions?.Count > 0)
                {
                    foreach (var permission in role.Permissions.ToList())
                    {
                        await _roleRepository.RemovePermissionFromRoleAsync(id, permission.Id);
                    }
                }

                // Add new permissions
                foreach (var permissionId in updateRoleDto.PermissionIds)
                {
                    var permission = await _permissionRepository.GetByIdAsync(permissionId);
                    if (permission != null)
                    {
                        await _roleRepository.AddPermissionToRoleAsync(id, permissionId);
                    }
                }
            }

            // Get updated role with permissions
            var updatedRole = await _roleRepository.GetWithPermissionsAsync(id);

            var roleDto = new RoleDto
            {
                Id = updatedRole!.Id,
                Name = updatedRole.Name,
                Description = updatedRole.Description,
                IsSystemRole = updatedRole.IsSystemRole,
                CreatedAt = updatedRole.CreatedAt,
                UpdatedAt = updatedRole.UpdatedAt,
                Permissions = updatedRole.Permissions?.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Resource = p.Resource,
                    Action = p.Action,
                    Description = p.Description,
                    Category = p.Category,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList() ?? new List<PermissionDto>()
            };

            return Ok(roleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(500, "An error occurred while updating the role");
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found");
            }

            // Prevent deletion of system roles
            if (role.IsSystemRole)
            {
                return BadRequest("System roles cannot be deleted");
            }

            await _roleRepository.DeleteAsync(role);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(500, "An error occurred while deleting the role");
        }
    }

    /// <summary>
    /// Get role by name
    /// </summary>
    [HttpGet("name/{name}")]
    public async Task<ActionResult<RoleDto>> GetRoleByName(string name)
    {
        try
        {
            var role = await _roleRepository.GetByNameAsync(name);
            if (role == null)
            {
                return NotFound($"Role with name '{name}' not found");
            }

            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                Permissions = role.Permissions?.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Resource = p.Resource,
                    Action = p.Action,
                    Description = p.Description,
                    Category = p.Category,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList() ?? new List<PermissionDto>()
            };

            return Ok(roleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role by name {RoleName}", name);
            return StatusCode(500, "An error occurred while retrieving the role");
        }
    }

    /// <summary>
    /// Add permission to role
    /// </summary>
    [HttpPost("{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> AddPermissionToRole(Guid roleId, Guid permissionId)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                return NotFound($"Role with ID {roleId} not found");
            }

            var permission = await _permissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
            {
                return NotFound($"Permission with ID {permissionId} not found");
            }

            // Prevent modification of system roles
            if (role.IsSystemRole)
            {
                return BadRequest("System roles cannot be modified");
            }

            await _roleRepository.AddPermissionToRoleAsync(roleId, permissionId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding permission {PermissionId} to role {RoleId}", permissionId, roleId);
            return StatusCode(500, "An error occurred while adding permission to role");
        }
    }

    /// <summary>
    /// Remove permission from role
    /// </summary>
    [HttpDelete("{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                return NotFound($"Role with ID {roleId} not found");
            }

            // Prevent modification of system roles
            if (role.IsSystemRole)
            {
                return BadRequest("System roles cannot be modified");
            }

            await _roleRepository.RemovePermissionFromRoleAsync(roleId, permissionId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission {PermissionId} from role {RoleId}", permissionId, roleId);
            return StatusCode(500, "An error occurred while removing permission from role");
        }
    }

    /// <summary>
    /// Get system roles
    /// </summary>
    [HttpGet("system")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetSystemRoles()
    {
        try
        {
            var roles = await _roleRepository.GetSystemRolesAsync();
            var roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Permissions = r.Permissions?.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Resource = p.Resource,
                    Action = p.Action,
                    Description = p.Description,
                    Category = p.Category,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList() ?? new List<PermissionDto>()
            });

            return Ok(roleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system roles");
            return StatusCode(500, "An error occurred while retrieving system roles");
        }
    }

    /// <summary>
    /// Get user roles (non-system roles)
    /// </summary>
    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetUserRoles()
    {
        try
        {
            var roles = await _roleRepository.GetUserRolesAsync();
            var roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Permissions = r.Permissions?.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Resource = p.Resource,
                    Action = p.Action,
                    Description = p.Description,
                    Category = p.Category,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList() ?? new List<PermissionDto>()
            });

            return Ok(roleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles");
            return StatusCode(500, "An error occurred while retrieving user roles");
        }
    }
}