using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
    {
        var tags = await _tagRepository.GetAllAsync();
        return tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Slug = t.Slug,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        });
    }

    public async Task<TagDto?> GetTagByIdAsync(Guid id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null) return null;

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            Slug = tag.Slug,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };
    }

    public async Task<TagDto?> GetTagBySlugAsync(string slug)
    {
        var tag = await _tagRepository.GetBySlugAsync(slug);
        if (tag == null) return null;

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            Slug = tag.Slug,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };
    }

    public async Task<IEnumerable<TagDto>> GetTagsByPostIdAsync(Guid postId)
    {
        var tags = await _tagRepository.GetTagsByPostIdAsync(postId);
        return tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Slug = t.Slug,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        });
    }

    public async Task<TagDto> CreateTagAsync(CreateTagDto createTagDto)
    {
        var tag = new Tag
        {
            Name = createTagDto.Name,
            Description = createTagDto.Description,
            Slug = string.IsNullOrEmpty(createTagDto.Slug) 
                ? GenerateSlug(createTagDto.Name) 
                : createTagDto.Slug
        };

        var createdTag = await _tagRepository.AddAsync(tag);

        return new TagDto
        {
            Id = createdTag.Id,
            Name = createdTag.Name,
            Description = createdTag.Description,
            Slug = createdTag.Slug,
            CreatedAt = createdTag.CreatedAt,
            UpdatedAt = createdTag.UpdatedAt
        };
    }

    public async Task<TagDto?> UpdateTagAsync(Guid id, UpdateTagDto updateTagDto)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null) return null;

        if (!string.IsNullOrEmpty(updateTagDto.Name))
            tag.Name = updateTagDto.Name;

        if (updateTagDto.Description != null)
            tag.Description = updateTagDto.Description;

        if (!string.IsNullOrEmpty(updateTagDto.Slug))
            tag.Slug = updateTagDto.Slug;

        var updatedTag = await _tagRepository.UpdateAsync(tag);

        return new TagDto
        {
            Id = updatedTag.Id,
            Name = updatedTag.Name,
            Description = updatedTag.Description,
            Slug = updatedTag.Slug,
            CreatedAt = updatedTag.CreatedAt,
            UpdatedAt = updatedTag.UpdatedAt
        };
    }

    public async Task<bool> DeleteTagAsync(Guid id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null) return false;

        if (tag.PostTags?.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete tag that has associated posts");
        }

        await _tagRepository.SoftDeleteAsync(tag);
        return true;
    }

    private string GenerateSlug(string name)
    {
        var normalized = name
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