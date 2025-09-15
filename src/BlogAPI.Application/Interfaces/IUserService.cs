using BlogAPI.Application.DTOs;

namespace BlogAPI.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto> CreateAsync(RegisterDto registerDto);
    Task<UserDto> UpdateAsync(Guid id, UserDto userDto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string email, string username);
    Task<bool> VerifyPasswordAsync(string email, string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}