using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        IPostRepository postRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository,
        IUserRepository userRepository,
        ILogger<PostsController> logger)
    {
        _postRepository = postRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all posts
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts([FromQuery] bool publishedOnly = false)
    {
        try
        {
            var posts = publishedOnly 
                ? await _postRepository.GetPublishedPostsAsync()
                : await _postRepository.GetAllAsync();

            var postDtos = await MapPostsToDto(posts);
            return Ok(postDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving posts");
            return StatusCode(500, "An error occurred while retrieving posts");
        }
    }

    /// <summary>
    /// Get post by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetPost(Guid id)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound($"Post with ID {id} not found");
            }

            var postDto = await MapPostToDto(post);
            return Ok(postDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving post {PostId}", id);
            return StatusCode(500, "An error occurred while retrieving the post");
        }
    }

    /// <summary>
    /// Get post by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<PostDto>> GetPostBySlug(string slug)
    {
        try
        {
            var post = await _postRepository.GetBySlugAsync(slug);
            if (post == null)
            {
                return NotFound($"Post with slug '{slug}' not found");
            }

            // Increment view count
            post.ViewCount++;
            await _postRepository.UpdateAsync(post);

            var postDto = await MapPostToDto(post);
            return Ok(postDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving post by slug {Slug}", slug);
            return StatusCode(500, "An error occurred while retrieving the post");
        }
    }

    /// <summary>
    /// Create a new post
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostDto>> CreatePost(CreatePostDto createPostDto)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(createPostDto.Title))
            {
                return BadRequest("Title is required");
            }

            if (string.IsNullOrWhiteSpace(createPostDto.Content))
            {
                return BadRequest("Content is required");
            }

            // Generate slug if not provided
            var slug = !string.IsNullOrWhiteSpace(createPostDto.Slug) 
                ? createPostDto.Slug 
                : GenerateSlug(createPostDto.Title);

            // Check if slug already exists
            var existingPost = await _postRepository.GetBySlugAsync(slug);
            if (existingPost != null)
            {
                return Conflict($"A post with slug '{slug}' already exists");
            }

            // Get current user ID (this would come from authentication context)
            // For now, we'll assume the first user is the author
            var users = await _userRepository.GetAllAsync();
            var author = users.FirstOrDefault();
            if (author == null)
            {
                return BadRequest("No users found. Please create a user first.");
            }

            var post = new Post
            {
                Title = createPostDto.Title,
                Content = createPostDto.Content,
                Summary = createPostDto.Summary,
                Slug = slug,
                IsPublished = createPostDto.IsPublished,
                FeaturedImage = createPostDto.FeaturedImage,
                ReadTimeMinutes = createPostDto.ReadTimeMinutes,
                ViewCount = 0,
                AuthorId = author.Id,
                PublishedAt = createPostDto.IsPublished ? DateTime.UtcNow : null
            };

            var createdPost = await _postRepository.AddAsync(post);

            // Add categories if provided
            if (createPostDto.CategoryIds?.Count > 0)
            {
                foreach (var categoryId in createPostDto.CategoryIds)
                {
                    var category = await _categoryRepository.GetByIdAsync(categoryId);
                    if (category != null)
                    {
                        await _postRepository.AddCategoryToPostAsync(createdPost.Id, categoryId);
                    }
                }
            }

            // Add tags if provided
            if (createPostDto.TagIds?.Count > 0)
            {
                foreach (var tagId in createPostDto.TagIds)
                {
                    var tag = await _tagRepository.GetByIdAsync(tagId);
                    if (tag != null)
                    {
                        await _postRepository.AddTagToPostAsync(createdPost.Id, tagId);
                    }
                }
            }

            var postDto = await MapPostToDto(createdPost);
            return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, postDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            return StatusCode(500, "An error occurred while creating the post");
        }
    }

    /// <summary>
    /// Update an existing post
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PostDto>> UpdatePost(Guid id, UpdatePostDto updatePostDto)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound($"Post with ID {id} not found");
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(updatePostDto.Title))
            {
                post.Title = updatePostDto.Title;
            }

            if (!string.IsNullOrWhiteSpace(updatePostDto.Content))
            {
                post.Content = updatePostDto.Content;
            }

            if (updatePostDto.Summary != null)
            {
                post.Summary = updatePostDto.Summary;
            }

            if (!string.IsNullOrWhiteSpace(updatePostDto.Slug))
            {
                // Check if new slug already exists (and it's not the current post)
                var existingPost = await _postRepository.GetBySlugAsync(updatePostDto.Slug);
                if (existingPost != null && existingPost.Id != id)
                {
                    return Conflict($"A post with slug '{updatePostDto.Slug}' already exists");
                }
                post.Slug = updatePostDto.Slug;
            }

            if (updatePostDto.IsPublished.HasValue)
            {
                post.IsPublished = updatePostDto.IsPublished.Value;
                if (updatePostDto.IsPublished.Value && post.PublishedAt == null)
                {
                    post.PublishedAt = DateTime.UtcNow;
                }
                else if (!updatePostDto.IsPublished.Value)
                {
                    post.PublishedAt = null;
                }
            }

            if (updatePostDto.FeaturedImage != null)
            {
                post.FeaturedImage = updatePostDto.FeaturedImage;
            }

            if (updatePostDto.ReadTimeMinutes.HasValue)
            {
                post.ReadTimeMinutes = updatePostDto.ReadTimeMinutes.Value;
            }

            await _postRepository.UpdateAsync(post);

            // Update categories if provided
            if (updatePostDto.CategoryIds != null)
            {
                // Remove existing categories
                await _postRepository.RemoveAllCategoriesFromPostAsync(id);

                // Add new categories
                foreach (var categoryId in updatePostDto.CategoryIds)
                {
                    var category = await _categoryRepository.GetByIdAsync(categoryId);
                    if (category != null)
                    {
                        await _postRepository.AddCategoryToPostAsync(id, categoryId);
                    }
                }
            }

            // Update tags if provided
            if (updatePostDto.TagIds != null)
            {
                // Remove existing tags
                await _postRepository.RemoveAllTagsFromPostAsync(id);

                // Add new tags
                foreach (var tagId in updatePostDto.TagIds)
                {
                    var tag = await _tagRepository.GetByIdAsync(tagId);
                    if (tag != null)
                    {
                        await _postRepository.AddTagToPostAsync(id, tagId);
                    }
                }
            }

            var updatedPost = await _postRepository.GetByIdAsync(id);
            var postDto = await MapPostToDto(updatedPost!);
            return Ok(postDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", id);
            return StatusCode(500, "An error occurred while updating the post");
        }
    }

    /// <summary>
    /// Move post to trash (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound($"Post with ID {id} not found");
            }

            await _postRepository.SoftDeleteAsync(post);
            return Ok(new { message = "Post moved to trash successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            return StatusCode(500, "An error occurred while deleting the post");
        }
    }

    /// <summary>
    /// Get all trashed posts
    /// </summary>
    [HttpGet("trash")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetTrashedPosts()
    {
        try
        {
            var posts = await _postRepository.GetTrashedAsync();
            var postDtos = posts.Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                Summary = p.Summary,
                Slug = p.Slug,
                IsPublished = p.IsPublished,
                PublishedAt = p.PublishedAt,
                FeaturedImage = p.FeaturedImage,
                ReadTimeMinutes = p.ReadTimeMinutes,
                ViewCount = p.ViewCount,
                AuthorId = p.AuthorId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsDeleted = p.IsDeleted,
                DeletedAt = p.DeletedAt,
                Categories = p.PostCategories?.Select(pc => new CategoryDto
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name,
                    Description = pc.Category.Description,
                    Slug = pc.Category.Slug,
                    CreatedAt = pc.Category.CreatedAt,
                    UpdatedAt = pc.Category.UpdatedAt
                }).ToList() ?? [],
                Tags = p.PostTags?.Select(pt => new TagDto
                {
                    Id = pt.Tag.Id,
                    Name = pt.Tag.Name,
                    Slug = pt.Tag.Slug,
                    CreatedAt = pt.Tag.CreatedAt,
                    UpdatedAt = pt.Tag.UpdatedAt
                }).ToList() ?? []
            });

            return Ok(postDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trashed posts");
            return StatusCode(500, "An error occurred while retrieving trashed posts");
        }
    }

    /// <summary>
    /// Restore a post from trash
    /// </summary>
    [HttpPatch("{id}/restore")]
    [Authorize]
    public async Task<IActionResult> RestorePost(Guid id)
    {
        try
        {
            var post = await _postRepository.GetTrashedByIdAsync(id);
            if (post == null)
            {
                return NotFound($"Trashed post with ID {id} not found");
            }

            await _postRepository.RestoreAsync(post);
            return Ok(new { message = "Post restored successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring post {PostId}", id);
            return StatusCode(500, "An error occurred while restoring the post");
        }
    }

    /// <summary>
    /// Permanently delete a post from trash
    /// </summary>
    [HttpDelete("{id}/permanent")]
    [Authorize]
    public async Task<IActionResult> PermanentlyDeletePost(Guid id)
    {
        try
        {
            var post = await _postRepository.GetTrashedByIdAsync(id);
            if (post == null)
            {
                return NotFound($"Trashed post with ID {id} not found");
            }

            await _postRepository.DeleteAsync(post);
            return Ok(new { message = "Post permanently deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting post {PostId}", id);
            return StatusCode(500, "An error occurred while permanently deleting the post");
        }
    }

    /// <summary>
    /// Get posts by category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsByCategory(Guid categoryId)
    {
        try
        {
            var posts = await _postRepository.GetByCategoryAsync(categoryId);
            var postDtos = await MapPostsToDto(posts);
            return Ok(postDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving posts by category {CategoryId}", categoryId);
            return StatusCode(500, "An error occurred while retrieving posts");
        }
    }

    /// <summary>
    /// Get posts by tag
    /// </summary>
    [HttpGet("tag/{tagId}")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsByTag(Guid tagId)
    {
        try
        {
            var posts = await _postRepository.GetByTagAsync(tagId);
            var postDtos = await MapPostsToDto(posts);
            return Ok(postDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving posts by tag {TagId}", tagId);
            return StatusCode(500, "An error occurred while retrieving posts");
        }
    }

    /// <summary>
    /// Get published posts
    /// </summary>
    [HttpGet("published")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPublishedPosts()
    {
        try
        {
            var posts = await _postRepository.GetPublishedPostsAsync();
            var postDtos = await MapPostsToDto(posts);
            return Ok(postDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving published posts");
            return StatusCode(500, "An error occurred while retrieving published posts");
        }
    }

    /// <summary>
    /// Generate URL-friendly slug from text
    /// </summary>
    private static string GenerateSlug(string text)
    {
        return text.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Trim('-');
    }

    /// <summary>
    /// Map a single post entity to DTO
    /// </summary>
    private async Task<PostDto> MapPostToDto(Post post)
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
            PublishedAt = post.PublishedAt,
            FeaturedImage = post.FeaturedImage,
            ReadTimeMinutes = post.ReadTimeMinutes,
            ViewCount = post.ViewCount,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            Author = new UserDto
            {
                Id = author!.Id,
                Username = author.Username,
                Email = author.Email,
                FirstName = author.FirstName,
                LastName = author.LastName,
                Bio = author.Bio,
                IsEmailConfirmed = author.IsEmailConfirmed,
                LastLoginAt = author.LastLoginAt,
                IsActive = author.IsActive,
                CreatedAt = author.CreatedAt,
                UpdatedAt = author.UpdatedAt
            },
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
            }).ToList()
        };
    }

    /// <summary>
    /// Map multiple post entities to DTOs
    /// </summary>
    private async Task<IEnumerable<PostDto>> MapPostsToDto(IEnumerable<Post> posts)
    {
        var postDtos = new List<PostDto>();
        foreach (var post in posts)
        {
            postDtos.Add(await MapPostToDto(post));
        }
        return postDtos;
    }
}