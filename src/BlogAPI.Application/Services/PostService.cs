using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;

    public PostService(
        IPostRepository postRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository,
        IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
    {
        var posts = await _postRepository.GetAllAsync();
        var postDtos = new List<PostDto>();
        
        foreach (var post in posts)
        {
            var postDto = await MapToDtoAsync(post);
            postDtos.Add(postDto);
        }
        
        return postDtos;
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        return post != null ? await MapToDtoAsync(post) : null;
    }

    public async Task<PostDto?> GetPostBySlugAsync(string slug)
    {
        var post = await _postRepository.GetBySlugAsync(slug);
        return post != null ? await MapToDtoAsync(post) : null;
    }

    public async Task<IEnumerable<PostDto>> GetPostsByCategoryAsync(Guid categoryId)
    {
        var posts = await _postRepository.GetByCategoryAsync(categoryId);
        var postDtos = new List<PostDto>();
        
        foreach (var post in posts)
        {
            var postDto = await MapToDtoAsync(post);
            postDtos.Add(postDto);
        }
        
        return postDtos;
    }

    public async Task<IEnumerable<PostDto>> GetPostsByTagAsync(Guid tagId)
    {
        var posts = await _postRepository.GetByTagAsync(tagId);
        var postDtos = new List<PostDto>();
        
        foreach (var post in posts)
        {
            var postDto = await MapToDtoAsync(post);
            postDtos.Add(postDto);
        }
        
        return postDtos;
    }

    public async Task<IEnumerable<PostDto>> GetPostsByAuthorAsync(Guid authorId)
    {
        var posts = await _postRepository.GetAllAsync();
        var authorPosts = posts.Where(p => p.AuthorId == authorId);
        var postDtos = new List<PostDto>();
        
        foreach (var post in authorPosts)
        {
            var postDto = await MapToDtoAsync(post);
            postDtos.Add(postDto);
        }
        
        return postDtos;
    }

    public async Task<IEnumerable<PostDto>> GetPublishedPostsAsync()
    {
        var posts = await _postRepository.GetPublishedPostsAsync();
        var postDtos = new List<PostDto>();
        
        foreach (var post in posts)
        {
            var postDto = await MapToDtoAsync(post);
            postDtos.Add(postDto);
        }
        
        return postDtos;
    }

    public async Task<IEnumerable<PostDto>> GetDraftPostsAsync()
    {
        var posts = await _postRepository.GetAllAsync();
        var draftPosts = posts.Where(p => !p.IsPublished);
        var postDtos = new List<PostDto>();
        
        foreach (var post in draftPosts)
        {
            var postDto = await MapToDtoAsync(post);
            postDtos.Add(postDto);
        }
        
        return postDtos;
    }

    public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto)
    {
        var post = new Post
        {
            Title = createPostDto.Title,
            Content = createPostDto.Content,
            Summary = createPostDto.Summary,
            Slug = string.IsNullOrEmpty(createPostDto.Slug) 
                ? GenerateSlug(createPostDto.Title) 
                : createPostDto.Slug,
            AuthorId = Guid.Empty, // Will be set by the controller from current user
            IsPublished = createPostDto.IsPublished,
            FeaturedImage = createPostDto.FeaturedImage,
            ReadTimeMinutes = createPostDto.ReadTimeMinutes,
            PublishedAt = createPostDto.IsPublished ? DateTime.UtcNow : null
        };

        var createdPost = await _postRepository.AddAsync(post);

        if (createPostDto.CategoryIds?.Count > 0)
        {
            await AssignCategoriesToPost(createdPost.Id, createPostDto.CategoryIds);
        }

        if (createPostDto.TagIds?.Count > 0)
        {
            await AssignTagsToPost(createdPost.Id, createPostDto.TagIds);
        }

        return await MapToDtoAsync(createdPost);
    }

    public async Task<PostDto?> UpdatePostAsync(Guid id, UpdatePostDto updatePostDto)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return null;

        if (!string.IsNullOrEmpty(updatePostDto.Title))
            post.Title = updatePostDto.Title;

        if (updatePostDto.Content != null)
            post.Content = updatePostDto.Content;

        if (updatePostDto.Summary != null)
            post.Summary = updatePostDto.Summary;

        if (!string.IsNullOrEmpty(updatePostDto.Slug))
            post.Slug = updatePostDto.Slug;

        if (updatePostDto.IsPublished.HasValue)
        {
            var wasPublished = post.IsPublished;
            post.IsPublished = updatePostDto.IsPublished.Value;
            
            if (!wasPublished && post.IsPublished)
            {
                post.PublishedAt = DateTime.UtcNow;
            }
        }

        if (updatePostDto.FeaturedImage != null)
            post.FeaturedImage = updatePostDto.FeaturedImage;

        if (updatePostDto.ReadTimeMinutes.HasValue)
            post.ReadTimeMinutes = updatePostDto.ReadTimeMinutes.Value;

        var updatedPost = await _postRepository.UpdateAsync(post);

        if (updatePostDto.CategoryIds != null)
        {
            await UpdatePostCategories(id, updatePostDto.CategoryIds);
        }

        if (updatePostDto.TagIds != null)
        {
            await UpdatePostTags(id, updatePostDto.TagIds);
        }

        return await MapToDtoAsync(updatedPost);
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return false;

        await _postRepository.SoftDeleteAsync(post);
        return true;
    }

    public async Task<bool> PublishPostAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return false;

        post.IsPublished = true;
        post.PublishedAt = DateTime.UtcNow;
        await _postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<bool> UnpublishPostAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return false;

        post.IsPublished = false;
        await _postRepository.UpdateAsync(post);
        return true;
    }

    private async Task AssignCategoriesToPost(Guid postId, List<Guid> categoryIds)
    {
        foreach (var categoryId in categoryIds)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category != null)
            {
                await _postRepository.AddCategoryToPostAsync(postId, categoryId);
            }
        }
    }

    private async Task AssignTagsToPost(Guid postId, List<Guid> tagIds)
    {
        foreach (var tagId in tagIds)
        {
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag != null)
            {
                await _postRepository.AddTagToPostAsync(postId, tagId);
            }
        }
    }

    private async Task UpdatePostCategories(Guid postId, List<Guid> categoryIds)
    {
        await _postRepository.RemoveAllCategoriesFromPostAsync(postId);
        await AssignCategoriesToPost(postId, categoryIds);
    }

    private async Task UpdatePostTags(Guid postId, List<Guid> tagIds)
    {
        await _postRepository.RemoveAllTagsFromPostAsync(postId);
        await AssignTagsToPost(postId, tagIds);
    }

    private async Task<PostDto> MapToDtoAsync(Post post)
    {
        var author = await _userRepository.GetByIdAsync(post.AuthorId);
        var categories = await _postRepository.GetCategoriesByPostAsync(post.Id);
        var tags = await _postRepository.GetTagsByPostAsync(post.Id);

        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Summary = post.Summary,
            Slug = post.Slug,
            IsPublished = post.IsPublished,
            FeaturedImage = post.FeaturedImage,
            ReadTimeMinutes = post.ReadTimeMinutes,
            ViewCount = post.ViewCount,
            AuthorId = post.AuthorId,
            Author = author != null ? new UserDto
            {
                Id = author.Id,
                Username = author.Username,
                Email = author.Email,
                FirstName = author.FirstName,
                LastName = author.LastName,
                CreatedAt = author.CreatedAt,
                UpdatedAt = author.UpdatedAt
            } : null,
            Categories = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            Tags = tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList(),
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            PublishedAt = post.PublishedAt
        };
    }

    private static string GenerateSlug(string title)
    {
        var normalized = title
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ş", "s")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("đ", "d")
            .Replace("Đ", "d")
            .Replace("à", "a")
            .Replace("á", "a")
            .Replace("ả", "a")
            .Replace("ã", "a")
            .Replace("ạ", "a")
            .Replace("ă", "a")
            .Replace("ằ", "a")
            .Replace("ắ", "a")
            .Replace("ẳ", "a")
            .Replace("ẵ", "a")
            .Replace("ặ", "a")
            .Replace("â", "a")
            .Replace("ầ", "a")
            .Replace("ấ", "a")
            .Replace("ẩ", "a")
            .Replace("ẫ", "a")
            .Replace("ậ", "a")
            .Replace("è", "e")
            .Replace("é", "e")
            .Replace("ẻ", "e")
            .Replace("ẽ", "e")
            .Replace("ẹ", "e")
            .Replace("ê", "e")
            .Replace("ề", "e")
            .Replace("ế", "e")
            .Replace("ể", "e")
            .Replace("ễ", "e")
            .Replace("ệ", "e")
            .Replace("ì", "i")
            .Replace("í", "i")
            .Replace("ỉ", "i")
            .Replace("ĩ", "i")
            .Replace("ị", "i")
            .Replace("ò", "o")
            .Replace("ó", "o")
            .Replace("ỏ", "o")
            .Replace("õ", "o")
            .Replace("ọ", "o")
            .Replace("ô", "o")
            .Replace("ồ", "o")
            .Replace("ố", "o")
            .Replace("ổ", "o")
            .Replace("ỗ", "o")
            .Replace("ộ", "o")
            .Replace("ơ", "o")
            .Replace("ờ", "o")
            .Replace("ớ", "o")
            .Replace("ở", "o")
            .Replace("ỡ", "o")
            .Replace("ợ", "o")
            .Replace("ù", "u")
            .Replace("ú", "u")
            .Replace("ủ", "u")
            .Replace("ũ", "u")
            .Replace("ụ", "u")
            .Replace("ư", "u")
            .Replace("ừ", "u")
            .Replace("ứ", "u")
            .Replace("ử", "u")
            .Replace("ữ", "u")
            .Replace("ự", "u")
            .Replace("ỳ", "y")
            .Replace("ý", "y")
            .Replace("ỷ", "y")
            .Replace("ỹ", "y")
            .Replace("ỵ", "y")
            .Normalize(System.Text.NormalizationForm.FormD);

        var result = new System.Text.StringBuilder();
        foreach (var c in normalized)
        {
            if (char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                result.Append(c);
            }
        }

        return System.Text.RegularExpressions.Regex.Replace(result.ToString(), @"[^a-z0-9\-]", "")
            .Replace("--", "-")
            .Trim('-');
    }
}