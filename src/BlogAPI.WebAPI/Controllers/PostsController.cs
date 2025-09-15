using BlogAPI.Application.DTOs;
using BlogAPI.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetAllPosts([FromQuery] bool publishedOnly = false)
    {
        var posts = publishedOnly 
            ? await _postService.GetPublishedPostsAsync()
            : await _postService.GetAllPostsAsync();
        
        return Ok(posts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostDto>> GetPost(Guid id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound();

        return Ok(post);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<PostDto>> GetPostBySlug(string slug)
    {
        var post = await _postService.GetPostBySlugAsync(slug);
        if (post == null)
            return NotFound();

        await _postService.IncrementViewCountAsync(post.Id);
        return Ok(post);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsByCategory(Guid categoryId)
    {
        var posts = await _postService.GetPostsByCategoryAsync(categoryId);
        return Ok(posts);
    }

    [HttpGet("tag/{tagId:guid}")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsByTag(Guid tagId)
    {
        var posts = await _postService.GetPostsByTagAsync(tagId);
        return Ok(posts);
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost(CreatePostDto createPostDto)
    {
        try
        {
            var post = await _postService.CreatePostAsync(createPostDto);
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PostDto>> UpdatePost(Guid id, UpdatePostDto updatePostDto)
    {
        var post = await _postService.UpdatePostAsync(id, updatePostDto);
        if (post == null)
            return NotFound();

        return Ok(post);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var result = await _postService.DeletePostAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}