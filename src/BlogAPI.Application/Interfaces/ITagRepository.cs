using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetBySlugAsync(string slug);
    Task<IEnumerable<Tag>> GetTagsByPostIdAsync(Guid postId);
}