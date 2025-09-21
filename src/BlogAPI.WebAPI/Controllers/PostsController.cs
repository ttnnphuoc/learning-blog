using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogAPI.WebAPI.Controllers;

public class PostsController : BaseApiController
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService, ILogger<PostsController> logger) 
        : base(logger)
    {
        _postService = postService;
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
                ? await _postService.GetPublishedPostsAsync()
                : await _postService.GetAllPostsAsync();

            return Ok(posts);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving posts");
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
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound($"Post with ID {id} not found");
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving post {PostId}", id);
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
            var post = await _postService.GetPostBySlugAsync(slug);
            if (post == null)
            {
                return NotFound($"Post with slug '{slug}' not found");
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving post by slug {Slug}", slug);
            return StatusCode(500, "An error occurred while retrieving the post");
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
            var posts = await _postService.GetPostsByCategoryAsync(categoryId);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving posts by category {CategoryId}", categoryId);
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
            var posts = await _postService.GetPostsByTagAsync(tagId);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving posts by tag {TagId}", tagId);
            return StatusCode(500, "An error occurred while retrieving posts");
        }
    }

    /// <summary>
    /// Get posts by author
    /// </summary>
    [HttpGet("author/{authorId}")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsByAuthor(Guid authorId)
    {
        try
        {
            var posts = await _postService.GetPostsByAuthorAsync(authorId);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving posts by author {AuthorId}", authorId);
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
            var posts = await _postService.GetPublishedPostsAsync();
            return Ok(posts);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving published posts");
            return StatusCode(500, "An error occurred while retrieving published posts");
        }
    }

    /// <summary>
    /// Get draft posts
    /// </summary>
    [HttpGet("drafts")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetDraftPosts()
    {
        try
        {
            var posts = await _postService.GetDraftPostsAsync();
            return Ok(posts);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving draft posts");
            return StatusCode(500, "An error occurred while retrieving draft posts");
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current user ID from JWT token
            var authorId = GetCurrentUserId();
            Logger.LogInformation("User claims: {Claims}", string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
            Logger.LogInformation("Creating post for author: {AuthorId}", authorId);

            var post = await _postService.CreatePostAsync(createPostDto, authorId);
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning("Unauthorized attempt to create post: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating post");
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current user authorization info
            var (currentUserId, isAdmin) = GetCurrentUserInfo();
            Logger.LogInformation("Updating post {PostId} for user: {UserId}, IsAdmin: {IsAdmin}", id, currentUserId, isAdmin);

            var post = await _postService.UpdatePostAsync(id, updatePostDto, currentUserId, isAdmin);
            if (post == null)
            {
                return NotFound($"Post with ID {id} not found");
            }

            return Ok(post);
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleAuthorizationException<PostDto>(ex, "update this post");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating post {PostId}", id);
            return StatusCode(500, "An error occurred while updating the post");
        }
    }

    /// <summary>
    /// Delete a post (move to trash)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        try
        {
            // Get current user authorization info
            var (currentUserId, isAdmin) = GetCurrentUserInfo();
            Logger.LogInformation("Deleting post {PostId} for user: {UserId}, IsAdmin: {IsAdmin}", id, currentUserId, isAdmin);

            var result = await _postService.DeletePostAsync(id, currentUserId, isAdmin);
            if (!result)
            {
                return NotFound($"Post with ID {id} not found");
            }

            return Ok(new { message = "Post moved to trash successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleAuthorizationException(ex, "delete this post");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting post {PostId}", id);
            return StatusCode(500, "An error occurred while deleting the post");
        }
    }

    /// <summary>
    /// Publish a post
    /// </summary>
    [HttpPost("{id}/publish")]
    [Authorize]
    public async Task<IActionResult> PublishPost(Guid id)
    {
        try
        {
            // Get current user authorization info
            var (currentUserId, isAdmin) = GetCurrentUserInfo();
            Logger.LogInformation("Publishing post {PostId} for user: {UserId}, IsAdmin: {IsAdmin}", id, currentUserId, isAdmin);

            var result = await _postService.PublishPostAsync(id, currentUserId, isAdmin);
            if (!result)
            {
                return NotFound($"Post with ID {id} not found");
            }

            return Ok(new { message = "Post published successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleAuthorizationException(ex, "publish this post");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error publishing post {PostId}", id);
            return StatusCode(500, "An error occurred while publishing the post");
        }
    }

    /// <summary>
    /// Unpublish a post
    /// </summary>
    [HttpPost("{id}/unpublish")]
    [Authorize]
    public async Task<IActionResult> UnpublishPost(Guid id)
    {
        try
        {
            // Get current user authorization info
            var (currentUserId, isAdmin) = GetCurrentUserInfo();
            Logger.LogInformation("Unpublishing post {PostId} for user: {UserId}, IsAdmin: {IsAdmin}", id, currentUserId, isAdmin);

            var result = await _postService.UnpublishPostAsync(id, currentUserId, isAdmin);
            if (!result)
            {
                return NotFound($"Post with ID {id} not found");
            }

            return Ok(new { message = "Post unpublished successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleAuthorizationException(ex, "unpublish this post");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error unpublishing post {PostId}", id);
            return StatusCode(500, "An error occurred while unpublishing the post");
        }
    }
}