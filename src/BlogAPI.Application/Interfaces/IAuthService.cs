using BlogAPI.Application.DTOs;

namespace BlogAPI.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginDto loginDto);
    Task<AuthResult> RegisterAsync(RegisterDto registerDto);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    string GenerateJwtToken(UserDto user);
    Task<string> GenerateRefreshTokenAsync(Guid userId);
}