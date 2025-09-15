namespace BlogAPI.Domain.Entities;

public class Post : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }
    public string FeaturedImage { get; set; } = string.Empty;
    public int ReadTimeMinutes { get; set; }
    public int ViewCount { get; set; } = 0;
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}