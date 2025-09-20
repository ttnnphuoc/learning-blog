namespace BlogAPI.Application.DTOs;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; } = false;
    public List<Guid> PermissionIds { get; set; } = new List<Guid>();
}

public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<Guid>? PermissionIds { get; set; }
}

public class AssignPermissionsDto
{
    public List<Guid> PermissionIds { get; set; } = new List<Guid>();
}