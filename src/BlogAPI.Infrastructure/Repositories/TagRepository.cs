using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Infrastructure.Repositories;

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(BlogDbContext context) : base(context)
    {
    }

    public async Task<Tag?> GetBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Slug == slug);
    }

    public async Task<IEnumerable<Tag>> GetTagsByPostIdAsync(Guid postId)
    {
        return await _dbSet
            .Where(t => t.PostTags.Any(pt => pt.PostId == postId))
            .ToListAsync();
    }
}