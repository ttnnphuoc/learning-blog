using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Infrastructure.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(BlogDbContext context) : base(context)
    {
    }

    public override async Task<Post?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public override async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _dbSet
            .Where(p => !p.IsDeleted)
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPublishedPostsAsync()
    {
        return await _dbSet
            .Where(p => !p.IsDeleted && p.IsPublished)
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Where(p => !p.IsDeleted && p.IsPublished && p.PostCategories.Any(pc => pc.CategoryId == categoryId))
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByTagAsync(Guid tagId)
    {
        return await _dbSet
            .Where(p => !p.IsDeleted && p.IsPublished && p.PostTags.Any(pt => pt.TagId == tagId))
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Where(p => !p.IsDeleted && p.PostCategories.Any(pc => pc.CategoryId == categoryId))
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetByTagAsync(Guid tagId)
    {
        return await _dbSet
            .Where(p => !p.IsDeleted && p.PostTags.Any(pt => pt.TagId == tagId))
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Where(p => !p.IsDeleted)
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task IncrementViewCountAsync(Guid postId)
    {
        var post = await _dbSet.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);
        if (post != null)
        {
            post.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Category>> GetCategoriesByPostAsync(Guid postId)
    {
        var post = await _dbSet
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.Id == postId);

        return post?.PostCategories.Select(pc => pc.Category) ?? [];
    }

    public async Task<IEnumerable<Tag>> GetTagsByPostAsync(Guid postId)
    {
        var post = await _dbSet
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == postId);

        return post?.PostTags.Select(pt => pt.Tag) ?? [];
    }

    public async Task AddCategoryToPostAsync(Guid postId, Guid categoryId)
    {
        var postCategory = new PostCategory
        {
            PostId = postId,
            CategoryId = categoryId
        };

        _context.PostCategories.Add(postCategory);
        await _context.SaveChangesAsync();
    }

    public async Task AddTagToPostAsync(Guid postId, Guid tagId)
    {
        var postTag = new PostTag
        {
            PostId = postId,
            TagId = tagId
        };

        _context.PostTags.Add(postTag);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAllCategoriesFromPostAsync(Guid postId)
    {
        var postCategories = await _context.PostCategories
            .Where(pc => pc.PostId == postId)
            .ToListAsync();

        _context.PostCategories.RemoveRange(postCategories);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAllTagsFromPostAsync(Guid postId)
    {
        var postTags = await _context.PostTags
            .Where(pt => pt.PostId == postId)
            .ToListAsync();

        _context.PostTags.RemoveRange(postTags);
        await _context.SaveChangesAsync();
    }
}