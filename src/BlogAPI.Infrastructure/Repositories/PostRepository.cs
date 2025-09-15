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
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPublishedPostsAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .Where(p => p.CategoryId == categoryId && p.IsPublished)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByTagAsync(Guid tagId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .Where(p => p.Tags.Any(t => t.Id == tagId) && p.IsPublished)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task IncrementViewCountAsync(Guid postId)
    {
        var post = await _dbSet.FindAsync(postId);
        if (post != null)
        {
            post.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }
}