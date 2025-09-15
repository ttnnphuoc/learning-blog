using BlogAPI.Application.DTOs;

namespace BlogAPI.Application.Services;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllTagsAsync();
    Task<TagDto?> GetTagByIdAsync(Guid id);
    Task<TagDto?> GetTagBySlugAsync(string slug);
    Task<IEnumerable<TagDto>> GetTagsByPostIdAsync(Guid postId);
    Task<TagDto> CreateTagAsync(CreateTagDto createTagDto);
    Task<TagDto?> UpdateTagAsync(Guid id, UpdateTagDto updateTagDto);
    Task<bool> DeleteTagAsync(Guid id);
}