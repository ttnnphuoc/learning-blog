namespace BlogAPI.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}