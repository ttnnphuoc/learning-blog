using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetPublishedPostsAsync();
    Task<IEnumerable<Post>> GetPostsByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Post>> GetPostsByTagAsync(Guid tagId);
    Task<Post?> GetBySlugAsync(string slug);
    Task IncrementViewCountAsync(Guid postId);
}