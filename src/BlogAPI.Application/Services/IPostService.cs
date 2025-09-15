using BlogAPI.Application.DTOs;

namespace BlogAPI.Application.Services;

public interface IPostService
{
    Task<IEnumerable<PostDto>> GetAllPostsAsync();
    Task<IEnumerable<PostDto>> GetPublishedPostsAsync();
    Task<PostDto?> GetPostByIdAsync(Guid id);
    Task<PostDto?> GetPostBySlugAsync(string slug);
    Task<IEnumerable<PostDto>> GetPostsByCategoryAsync(Guid categoryId);
    Task<IEnumerable<PostDto>> GetPostsByTagAsync(Guid tagId);
    Task<PostDto> CreatePostAsync(CreatePostDto createPostDto);
    Task<PostDto?> UpdatePostAsync(Guid id, UpdatePostDto updatePostDto);
    Task<bool> DeletePostAsync(Guid id);
    Task IncrementViewCountAsync(Guid id);
}