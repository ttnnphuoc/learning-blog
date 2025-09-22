using BlogAPI.Application.DTOs;

namespace BlogAPI.Application.Interfaces;

public interface IPostService
{
    Task<PaginatedResponse<PostDto>> GetPostsAsync(PostQueryParams queryParams);
    Task<IEnumerable<PostDto>> GetAllPostsAsync();
    Task<PostDto?> GetPostByIdAsync(Guid id);
    Task<PostDto?> GetPostBySlugAsync(string slug);
    Task<IEnumerable<PostDto>> GetPostsByCategoryAsync(Guid categoryId);
    Task<IEnumerable<PostDto>> GetPostsByTagAsync(Guid tagId);
    Task<IEnumerable<PostDto>> GetPostsByAuthorAsync(Guid authorId);
    Task<IEnumerable<PostDto>> GetPublishedPostsAsync();
    Task<IEnumerable<PostDto>> GetDraftPostsAsync();
    Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, Guid authorId);
    Task<PostDto?> UpdatePostAsync(Guid id, UpdatePostDto updatePostDto, Guid currentUserId, bool isAdmin = false);
    Task<bool> DeletePostAsync(Guid id, Guid currentUserId, bool isAdmin = false);
    Task<bool> PublishPostAsync(Guid id, Guid currentUserId, bool isAdmin = false);
    Task<bool> UnpublishPostAsync(Guid id, Guid currentUserId, bool isAdmin = false);
}