namespace BlogAPI.Application.DTOs;

public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string FeaturedImage { get; set; } = string.Empty;
    public int ReadTimeMinutes { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public CategoryDto Category { get; set; } = null!;
    public List<TagDto> Tags { get; set; } = new();
}

public class CreatePostDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public string FeaturedImage { get; set; } = string.Empty;
    public int ReadTimeMinutes { get; set; }
    public Guid CategoryId { get; set; }
    public List<Guid> TagIds { get; set; } = new();
}

public class UpdatePostDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? Slug { get; set; }
    public bool? IsPublished { get; set; }
    public string? FeaturedImage { get; set; }
    public int? ReadTimeMinutes { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid>? TagIds { get; set; }
}