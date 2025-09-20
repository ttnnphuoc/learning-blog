using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetPublishedPostsAsync();
    Task<IEnumerable<Post>> GetPostsByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Post>> GetPostsByTagAsync(Guid tagId);
    Task<IEnumerable<Post>> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Post>> GetByTagAsync(Guid tagId);
    Task<Post?> GetBySlugAsync(string slug);
    Task IncrementViewCountAsync(Guid postId);
    Task<IEnumerable<Category>> GetCategoriesByPostAsync(Guid postId);
    Task<IEnumerable<Tag>> GetTagsByPostAsync(Guid postId);
    Task AddCategoryToPostAsync(Guid postId, Guid categoryId);
    Task AddTagToPostAsync(Guid postId, Guid tagId);
    Task RemoveAllCategoriesFromPostAsync(Guid postId);
    Task RemoveAllTagsFromPostAsync(Guid postId);
}