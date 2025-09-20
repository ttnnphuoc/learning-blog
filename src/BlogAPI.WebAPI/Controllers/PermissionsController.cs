using Microsoft.AspNetCore.Mvc;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IPermissionRepository permissionRepository, ILogger<PermissionsController> logger)
    {
        _permissionRepository = permissionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all permissions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissions()
    {
        try
        {
            var permissions = await _permissionRepository.GetAllAsync();
            var permissionDtos = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Resource = p.Resource,
                Action = p.Action,
                Description = p.Description,
                Category = p.Category,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            return Ok(permissionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions");
            return StatusCode(500, "An error occurred while retrieving permissions");
        }
    }

    /// <summary>
    /// Get permission by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PermissionDto>> GetPermission(Guid id)
    {
        try
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
            {
                return NotFound($"Permission with ID {id} not found");
            }

            var permissionDto = new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Resource = permission.Resource,
                Action = permission.Action,
                Description = permission.Description,
                Category = permission.Category,
                CreatedAt = permission.CreatedAt,
                UpdatedAt = permission.UpdatedAt
            };

            return Ok(permissionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permission {PermissionId}", id);
            return StatusCode(500, "An error occurred while retrieving the permission");
        }
    }

    /// <summary>
    /// Create a new permission
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PermissionDto>> CreatePermission(CreatePermissionDto createPermissionDto)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(createPermissionDto.Name))
            {
                return BadRequest("Permission name is required");
            }

            if (string.IsNullOrWhiteSpace(createPermissionDto.Resource))
            {
                return BadRequest("Resource is required");
            }

            if (string.IsNullOrWhiteSpace(createPermissionDto.Action))
            {
                return BadRequest("Action is required");
            }

            // Check if permission already exists
            var existingPermission = await _permissionRepository.GetByNameAsync(createPermissionDto.Name);
            if (existingPermission != null)
            {
                return Conflict($"A permission with name '{createPermissionDto.Name}' already exists");
            }

            var permission = new Permission
            {
                Name = createPermissionDto.Name,
                Resource = createPermissionDto.Resource,
                Action = createPermissionDto.Action,
                Description = createPermissionDto.Description,
                Category = createPermissionDto.Category
            };

            var createdPermission = await _permissionRepository.AddAsync(permission);

            var permissionDto = new PermissionDto
            {
                Id = createdPermission.Id,
                Name = createdPermission.Name,
                Resource = createdPermission.Resource,
                Action = createdPermission.Action,
                Description = createdPermission.Description,
                Category = createdPermission.Category,
                CreatedAt = createdPermission.CreatedAt,
                UpdatedAt = createdPermission.UpdatedAt
            };

            return CreatedAtAction(nameof(GetPermission), new { id = createdPermission.Id }, permissionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating permission");
            return StatusCode(500, "An error occurred while creating the permission");
        }
    }

    /// <summary>
    /// Update an existing permission
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PermissionDto>> UpdatePermission(Guid id, UpdatePermissionDto updatePermissionDto)
    {
        try
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
            {
                return NotFound($"Permission with ID {id} not found");
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(updatePermissionDto.Name))
            {
                // Check if new name already exists (and it's not the current permission)
                var existingPermission = await _permissionRepository.GetByNameAsync(updatePermissionDto.Name);
                if (existingPermission != null && existingPermission.Id != id)
                {
                    return Conflict($"A permission with name '{updatePermissionDto.Name}' already exists");
                }
                permission.Name = updatePermissionDto.Name;
            }

            if (!string.IsNullOrWhiteSpace(updatePermissionDto.Resource))
            {
                permission.Resource = updatePermissionDto.Resource;
            }

            if (!string.IsNullOrWhiteSpace(updatePermissionDto.Action))
            {
                permission.Action = updatePermissionDto.Action;
            }

            if (updatePermissionDto.Description != null)
            {
                permission.Description = updatePermissionDto.Description;
            }

            if (!string.IsNullOrWhiteSpace(updatePermissionDto.Category))
            {
                permission.Category = updatePermissionDto.Category;
            }

            await _permissionRepository.UpdateAsync(permission);

            var permissionDto = new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Resource = permission.Resource,
                Action = permission.Action,
                Description = permission.Description,
                Category = permission.Category,
                CreatedAt = permission.CreatedAt,
                UpdatedAt = permission.UpdatedAt
            };

            return Ok(permissionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permission {PermissionId}", id);
            return StatusCode(500, "An error occurred while updating the permission");
        }
    }

    /// <summary>
    /// Delete a permission
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        try
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
            {
                return NotFound($"Permission with ID {id} not found");
            }

            await _permissionRepository.DeleteAsync(permission);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting permission {PermissionId}", id);
            return StatusCode(500, "An error occurred while deleting the permission");
        }
    }

    /// <summary>
    /// Get permission by name
    /// </summary>
    [HttpGet("name/{name}")]
    public async Task<ActionResult<PermissionDto>> GetPermissionByName(string name)
    {
        try
        {
            var permission = await _permissionRepository.GetByNameAsync(name);
            if (permission == null)
            {
                return NotFound($"Permission with name '{name}' not found");
            }

            var permissionDto = new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Resource = permission.Resource,
                Action = permission.Action,
                Description = permission.Description,
                Category = permission.Category,
                CreatedAt = permission.CreatedAt,
                UpdatedAt = permission.UpdatedAt
            };

            return Ok(permissionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permission by name {PermissionName}", name);
            return StatusCode(500, "An error occurred while retrieving the permission");
        }
    }

    /// <summary>
    /// Get permissions by resource
    /// </summary>
    [HttpGet("resource/{resource}")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissionsByResource(string resource)
    {
        try
        {
            var permissions = await _permissionRepository.GetByResourceAsync(resource);
            var permissionDtos = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Resource = p.Resource,
                Action = p.Action,
                Description = p.Description,
                Category = p.Category,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            return Ok(permissionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions by resource {Resource}", resource);
            return StatusCode(500, "An error occurred while retrieving permissions");
        }
    }

    /// <summary>
    /// Get permissions by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissionsByCategory(string category)
    {
        try
        {
            var permissions = await _permissionRepository.GetByCategoryAsync(category);
            var permissionDtos = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Resource = p.Resource,
                Action = p.Action,
                Description = p.Description,
                Category = p.Category,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            return Ok(permissionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions by category {Category}", category);
            return StatusCode(500, "An error occurred while retrieving permissions");
        }
    }
}