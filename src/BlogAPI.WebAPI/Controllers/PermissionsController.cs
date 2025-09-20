using Microsoft.AspNetCore.Mvc;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;

namespace BlogAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
    {
        _permissionService = permissionService;
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
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
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
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
            {
                return NotFound($"Permission with ID {id} not found");
            }

            return Ok(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permission {PermissionId}", id);
            return StatusCode(500, "An error occurred while retrieving the permission");
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
            var permission = await _permissionService.GetPermissionByNameAsync(name);
            if (permission == null)
            {
                return NotFound($"Permission with name '{name}' not found");
            }

            return Ok(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permission by name {PermissionName}", name);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var permission = await _permissionService.CreatePermissionAsync(createPermissionDto);
            return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, permission);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var permission = await _permissionService.UpdatePermissionAsync(id, updatePermissionDto);
            if (permission == null)
            {
                return NotFound($"Permission with ID {id} not found");
            }

            return Ok(permission);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permission {PermissionId}", id);
            return StatusCode(500, "An error occurred while updating the permission");
        }
    }

    /// <summary>
    /// Delete a permission (move to trash)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        try
        {
            var result = await _permissionService.DeletePermissionAsync(id);
            if (!result)
            {
                return NotFound($"Permission with ID {id} not found");
            }

            return Ok(new { message = "Permission moved to trash successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting permission {PermissionId}", id);
            return StatusCode(500, "An error occurred while deleting the permission");
        }
    }
}