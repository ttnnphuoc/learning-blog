using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Common.Utils;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Services;

public class PostService(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    IUserRepository userRepository) : IPostService
{
    private readonly IPostRepository _postRepository = postRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<PaginatedResponse<PostDto>> GetPostsAsync(PostQueryParams queryParams)
    {
        try
        {
            // Get all posts first (we'll add repository-level pagination later)
            var allPosts = await _postRepository.GetAllAsync();
            
            // Apply filters
            var filteredPosts = allPosts.AsQueryable();
            
            if (queryParams.PublishedOnly.HasValue)
            {
                filteredPosts = queryParams.PublishedOnly.Value 
                    ? filteredPosts.Where(p => p.IsPublished)
                    : filteredPosts.Where(p => !p.IsPublished);
            }
            
            if (queryParams.AuthorId.HasValue)
            {
                filteredPosts = filteredPosts.Where(p => p.AuthorId == queryParams.AuthorId.Value);
            }
            
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var searchTerm = queryParams.Search;
                filteredPosts = filteredPosts.Where(p => 
                    p.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (p.Summary != null && p.Summary.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }
            
            // Order by creation date (newest first)
            filteredPosts = filteredPosts.OrderByDescending(p => p.CreatedAt);
            
            // Calculate pagination
            var totalItems = filteredPosts.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize);
            var skip = (queryParams.Page - 1) * queryParams.PageSize;
            
            // Apply pagination
            var pagedPosts = filteredPosts.Skip(skip).Take(queryParams.PageSize).ToList();
            
            // Map to DTOs
            List<PostDto> postDtos = [];
            foreach (var post in pagedPosts)
            {
                var postDto = await MapToDtoAsync(post);
                postDtos.Add(postDto);
            }
            
            return new PaginatedResponse<PostDto>
            {
                Success = true,
                Data = postDtos,
                Pagination = new PaginationMeta
                {
                    CurrentPage = queryParams.Page,
                    PageSize = queryParams.PageSize,
                    TotalPages = totalPages,
                    TotalItems = totalItems,
                    HasNext = queryParams.Page < totalPages,
                    HasPrevious = queryParams.Page > 1
                }
            };
        }
        catch (Exception ex)
        {
            return new PaginatedResponse<PostDto>
            {
                Success = false,
                Error = ex.Message,
                Data = [],
                Pagination = new()
            };
        }
    }

    public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
    {
        var posts = await _postRepository.GetAllAsync();
        List<PostDto> postDtos = [];
        
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
        List<PostDto> postDtos = [];
        
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
        List<PostDto> postDtos = [];
        
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
        List<PostDto> postDtos = [];
        
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
        List<PostDto> postDtos = [];
        
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
        List<PostDto> postDtos = [];
        
        foreach (var post in draftPosts)
        {
            var postDto = await MapToDtoAsync(post);
            postDtos.Add(postDto);
        }
        
        return postDtos;
    }

    public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, Guid authorId)
    {
        var post = new Post
        {
            Title = createPostDto.Title,
            Content = createPostDto.Content,
            Summary = createPostDto.Summary,
            Slug = string.IsNullOrEmpty(createPostDto.Slug) 
                ? SlugGenerator.GenerateSlug(createPostDto.Title) 
                : createPostDto.Slug,
            AuthorId = authorId,
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

    public async Task<PostDto?> UpdatePostAsync(Guid id, UpdatePostDto updatePostDto, Guid currentUserId, bool isAdmin = false)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return null;

        // Check if user is authorized to update this post
        if (!isAdmin && post.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only update your own posts");
        }

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

    public async Task<bool> DeletePostAsync(Guid id, Guid currentUserId, bool isAdmin = false)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return false;

        // Check if user is authorized to delete this post
        if (!isAdmin && post.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own posts");
        }

        await _postRepository.SoftDeleteAsync(post);
        return true;
    }

    public async Task<bool> PublishPostAsync(Guid id, Guid currentUserId, bool isAdmin = false)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return false;

        // Check if user is authorized to publish this post
        if (!isAdmin && post.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only publish your own posts");
        }

        post.IsPublished = true;
        post.PublishedAt = DateTime.UtcNow;
        await _postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<bool> UnpublishPostAsync(Guid id, Guid currentUserId, bool isAdmin = false)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return false;

        // Check if user is authorized to unpublish this post
        if (!isAdmin && post.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only unpublish your own posts");
        }

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
            Categories = [.. categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })],
            Tags = [.. tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })],
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            PublishedAt = post.PublishedAt
        };
    }

}