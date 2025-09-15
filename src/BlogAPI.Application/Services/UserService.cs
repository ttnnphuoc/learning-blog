using BCrypt.Net;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetWithRolesAsync(id);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserDto);
    }

    public async Task<UserDto> CreateAsync(RegisterDto registerDto)
    {
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = HashPassword(registerDto.Password),
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            IsActive = true,
            IsEmailConfirmed = false
        };

        var createdUser = await _userRepository.AddAsync(user);
        return MapToUserDto(createdUser);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UserDto userDto)
    {
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
            throw new ArgumentException("User not found");

        existingUser.Username = userDto.Username;
        existingUser.Email = userDto.Email;
        existingUser.FirstName = userDto.FirstName;
        existingUser.LastName = userDto.LastName;
        existingUser.IsActive = userDto.IsActive;
        existingUser.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(existingUser);
        return MapToUserDto(existingUser);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return false;
        
        await _userRepository.DeleteAsync(user);
        return true;
    }

    public async Task<bool> ExistsAsync(string email, string username)
    {
        return await _userRepository.ExistsAsync(email, username);
    }

    public async Task<bool> VerifyPasswordAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
            return false;

        return VerifyPassword(password, user.PasswordHash);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsEmailConfirmed = user.IsEmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles?.Select(ur => new RoleDto
            {
                Id = ur.Role.Id,
                Name = ur.Role.Name,
                Description = ur.Role.Description,
                CreatedAt = ur.Role.CreatedAt
            }) ?? new List<RoleDto>()
        };
    }
}