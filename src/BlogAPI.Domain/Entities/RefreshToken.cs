namespace BlogAPI.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public Guid UserId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}