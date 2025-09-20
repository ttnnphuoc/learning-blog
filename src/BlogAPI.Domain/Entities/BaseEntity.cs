namespace BlogAPI.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Soft Delete Fields
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}