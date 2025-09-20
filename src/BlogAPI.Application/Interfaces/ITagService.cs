using BlogAPI.Application.DTOs;

namespace BlogAPI.Application.Interfaces;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllTagsAsync();
    Task<TagDto?> GetTagByIdAsync(Guid id);
    Task<TagDto?> GetTagBySlugAsync(string slug);
    Task<IEnumerable<TagDto>> GetTagsByPostIdAsync(Guid postId);
    Task<TagDto> CreateTagAsync(CreateOrUpdateTagDto createTagDto);
    Task<TagDto?> UpdateTagAsync(Guid id, CreateOrUpdateTagDto updateTagDto);
    Task<bool> DeleteTagAsync(Guid id);
}