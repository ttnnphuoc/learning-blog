using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Common.Utils;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Slug = c.Slug,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug)
    {
        var category = await _categoryRepository.GetBySlugAsync(slug);
        if (category == null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        var category = new Category
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description,
            Slug = string.IsNullOrEmpty(createCategoryDto.Slug) 
                ? SlugGenerator.GenerateSlug(createCategoryDto.Name) 
                : createCategoryDto.Slug
        };

        var createdCategory = await _categoryRepository.AddAsync(category);

        return new CategoryDto
        {
            Id = createdCategory.Id,
            Name = createdCategory.Name,
            Description = createdCategory.Description,
            Slug = createdCategory.Slug,
            CreatedAt = createdCategory.CreatedAt,
            UpdatedAt = createdCategory.UpdatedAt
        };
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return null;

        // Update fields if provided
        if (!string.IsNullOrEmpty(updateCategoryDto.Name))
            category.Name = updateCategoryDto.Name;

        if (updateCategoryDto.Description != null)
            category.Description = updateCategoryDto.Description;

        if (!string.IsNullOrEmpty(updateCategoryDto.Slug))
            category.Slug = updateCategoryDto.Slug;

        var updatedCategory = await _categoryRepository.UpdateAsync(category);

        return new CategoryDto
        {
            Id = updatedCategory.Id,
            Name = updatedCategory.Name,
            Description = updatedCategory.Description,
            Slug = updatedCategory.Slug,
            CreatedAt = updatedCategory.CreatedAt,
            UpdatedAt = updatedCategory.UpdatedAt
        };
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return false;

        // Check if category has posts (business rule)
        if (category.PostCategories?.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete category that has associated posts");
        }

        await _categoryRepository.SoftDeleteAsync(category);
        return true;
    }

}