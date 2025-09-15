using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;

    public PostService(IPostRepository postRepository, ICategoryRepository categoryRepository, ITagRepository tagRepository)
    {
        _postRepository = postRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
    {
        var posts = await _postRepository.GetAllAsync();
        return posts.Select(MapToDto);
    }

    public async Task<IEnumerable<PostDto>> GetPublishedPostsAsync()
    {
        var posts = await _postRepository.GetPublishedPostsAsync();
        return posts.Select(MapToDto);
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        return post != null ? MapToDto(post) : null;
    }

    public async Task<PostDto?> GetPostBySlugAsync(string slug)
    {
        var post = await _postRepository.GetBySlugAsync(slug);
        return post != null ? MapToDto(post) : null;
    }

    public async Task<IEnumerable<PostDto>> GetPostsByCategoryAsync(Guid categoryId)
    {
        var posts = await _postRepository.GetPostsByCategoryAsync(categoryId);
        return posts.Select(MapToDto);
    }

    public async Task<IEnumerable<PostDto>> GetPostsByTagAsync(Guid tagId)
    {
        var posts = await _postRepository.GetPostsByTagAsync(tagId);
        return posts.Select(MapToDto);
    }

    public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto)
    {
        var category = await _categoryRepository.GetByIdAsync(createPostDto.CategoryId);
        if (category == null)
            throw new ArgumentException("Category not found");

        var tags = new List<Tag>();
        foreach (var tagId in createPostDto.TagIds)
        {
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag != null)
                tags.Add(tag);
        }

        var post = new Post
        {
            Title = createPostDto.Title,
            Content = createPostDto.Content,
            Summary = createPostDto.Summary,
            Slug = createPostDto.Slug,
            IsPublished = createPostDto.IsPublished,
            FeaturedImage = createPostDto.FeaturedImage,
            ReadTimeMinutes = createPostDto.ReadTimeMinutes,
            CategoryId = createPostDto.CategoryId,
            PublishedAt = createPostDto.IsPublished ? DateTime.UtcNow : null,
            Tags = tags
        };

        var createdPost = await _postRepository.AddAsync(post);
        return MapToDto(createdPost);
    }

    public async Task<PostDto?> UpdatePostAsync(Guid id, UpdatePostDto updatePostDto)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
            return null;

        if (!string.IsNullOrEmpty(updatePostDto.Title))
            post.Title = updatePostDto.Title;
        
        if (!string.IsNullOrEmpty(updatePostDto.Content))
            post.Content = updatePostDto.Content;
        
        if (!string.IsNullOrEmpty(updatePostDto.Summary))
            post.Summary = updatePostDto.Summary;
        
        if (!string.IsNullOrEmpty(updatePostDto.Slug))
            post.Slug = updatePostDto.Slug;
        
        if (updatePostDto.IsPublished.HasValue)
        {
            post.IsPublished = updatePostDto.IsPublished.Value;
            if (updatePostDto.IsPublished.Value && post.PublishedAt == null)
                post.PublishedAt = DateTime.UtcNow;
        }
        
        if (!string.IsNullOrEmpty(updatePostDto.FeaturedImage))
            post.FeaturedImage = updatePostDto.FeaturedImage;
        
        if (updatePostDto.ReadTimeMinutes.HasValue)
            post.ReadTimeMinutes = updatePostDto.ReadTimeMinutes.Value;
        
        if (updatePostDto.CategoryId.HasValue)
            post.CategoryId = updatePostDto.CategoryId.Value;

        if (updatePostDto.TagIds != null)
        {
            var tags = new List<Tag>();
            foreach (var tagId in updatePostDto.TagIds)
            {
                var tag = await _tagRepository.GetByIdAsync(tagId);
                if (tag != null)
                    tags.Add(tag);
            }
            post.Tags = tags;
        }

        post.UpdatedAt = DateTime.UtcNow;
        await _postRepository.UpdateAsync(post);
        
        return MapToDto(post);
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
            return false;

        await _postRepository.DeleteAsync(post);
        return true;
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        await _postRepository.IncrementViewCountAsync(id);
    }

    private static PostDto MapToDto(Post post)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Summary = post.Summary,
            Slug = post.Slug,
            IsPublished = post.IsPublished,
            PublishedAt = post.PublishedAt,
            FeaturedImage = post.FeaturedImage,
            ReadTimeMinutes = post.ReadTimeMinutes,
            ViewCount = post.ViewCount,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            Category = new CategoryDto
            {
                Id = post.Category.Id,
                Name = post.Category.Name,
                Description = post.Category.Description,
                Slug = post.Category.Slug,
                CreatedAt = post.Category.CreatedAt,
                UpdatedAt = post.Category.UpdatedAt
            },
            Tags = post.Tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList()
        };
    }
}