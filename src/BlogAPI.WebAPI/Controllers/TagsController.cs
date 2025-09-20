using Microsoft.AspNetCore.Mvc;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ITagRepository tagRepository, ILogger<TagsController> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
    {
        try
        {
            var tags = await _tagRepository.GetAllAsync();
            var tagDtos = tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            });

            return Ok(tagDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags");
            return StatusCode(500, "An error occurred while retrieving tags");
        }
    }

    /// <summary>
    /// Get tag by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(Guid id)
    {
        try
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return NotFound($"Tag with ID {id} not found");
            }

            var tagDto = new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt
            };

            return Ok(tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag {TagId}", id);
            return StatusCode(500, "An error occurred while retrieving the tag");
        }
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(createTagDto.Name))
            {
                return BadRequest("Tag name is required");
            }

            // Generate slug if not provided
            var slug = !string.IsNullOrWhiteSpace(createTagDto.Slug) 
                ? createTagDto.Slug 
                : GenerateSlug(createTagDto.Name);

            // Check if slug already exists
            var existingTag = await _tagRepository.GetBySlugAsync(slug);
            if (existingTag != null)
            {
                return Conflict($"A tag with slug '{slug}' already exists");
            }

            var tag = new Tag
            {
                Name = createTagDto.Name,
                Slug = slug
            };

            var createdTag = await _tagRepository.AddAsync(tag);

            var tagDto = new TagDto
            {
                Id = createdTag.Id,
                Name = createdTag.Name,
                Slug = createdTag.Slug,
                CreatedAt = createdTag.CreatedAt,
                UpdatedAt = createdTag.UpdatedAt
            };

            return CreatedAtAction(nameof(GetTag), new { id = createdTag.Id }, tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tag");
            return StatusCode(500, "An error occurred while creating the tag");
        }
    }

    /// <summary>
    /// Update an existing tag
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TagDto>> UpdateTag(Guid id, UpdateTagDto updateTagDto)
    {
        try
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return NotFound($"Tag with ID {id} not found");
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(updateTagDto.Name))
            {
                tag.Name = updateTagDto.Name;
            }

            if (!string.IsNullOrWhiteSpace(updateTagDto.Slug))
            {
                // Check if new slug already exists (and it's not the current tag)
                var existingTag = await _tagRepository.GetBySlugAsync(updateTagDto.Slug);
                if (existingTag != null && existingTag.Id != id)
                {
                    return Conflict($"A tag with slug '{updateTagDto.Slug}' already exists");
                }
                tag.Slug = updateTagDto.Slug;
            }

            var updatedTag = await _tagRepository.UpdateAsync(tag);

            var tagDto = new TagDto
            {
                Id = updatedTag.Id,
                Name = updatedTag.Name,
                Slug = updatedTag.Slug,
                CreatedAt = updatedTag.CreatedAt,
                UpdatedAt = updatedTag.UpdatedAt
            };

            return Ok(tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag {TagId}", id);
            return StatusCode(500, "An error occurred while updating the tag");
        }
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        try
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return NotFound($"Tag with ID {id} not found");
            }

            await _tagRepository.DeleteAsync(tag);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag {TagId}", id);
            return StatusCode(500, "An error occurred while deleting the tag");
        }
    }

    /// <summary>
    /// Get tag by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<TagDto>> GetTagBySlug(string slug)
    {
        try
        {
            var tag = await _tagRepository.GetBySlugAsync(slug);
            if (tag == null)
            {
                return NotFound($"Tag with slug '{slug}' not found");
            }

            var tagDto = new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt
            };

            return Ok(tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag by slug {Slug}", slug);
            return StatusCode(500, "An error occurred while retrieving the tag");
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
}