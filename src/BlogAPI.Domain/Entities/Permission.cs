namespace BlogAPI.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // Posts, Categories, Users, etc.
    public string Action { get; set; } = string.Empty; // Create, Read, Update, Delete
    public string Description { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}