using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Common.Utils;
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

    public async Task<TagDto> CreateTagAsync(CreateOrUpdateTagDto createTagDto)
    {
        var tag = new Tag
        {
            Name = createTagDto.Name,
            Description = createTagDto.Description,
            Slug = string.IsNullOrEmpty(createTagDto.Slug) 
                ? SlugGenerator.GenerateSlug(createTagDto.Name) 
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

    public async Task<TagDto?> UpdateTagAsync(Guid id, CreateOrUpdateTagDto updateTagDto)
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

}