using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpirationMinutes;

    public AuthService(
        IUserService userService,
        IRefreshTokenRepository refreshTokenRepository,
        string jwtSecret,
        string jwtIssuer,
        string jwtAudience,
        int jwtExpirationMinutes = 1440)
    {
        _userService = userService;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtSecret = jwtSecret;
        _jwtIssuer = jwtIssuer;
        _jwtAudience = jwtAudience;
        _jwtExpirationMinutes = jwtExpirationMinutes;
    }

    public async Task<AuthResult> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await _userService.GetByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new AuthResult { Success = false, Error = "Invalid email or password" };
            }

            var isValidPassword = await _userService.VerifyPasswordAsync(loginDto.Email, loginDto.Password);
            if (!isValidPassword)
            {
                return new AuthResult { Success = false, Error = "Invalid email or password" };
            }

            if (!user.IsActive)
            {
                return new AuthResult { Success = false, Error = "Account is deactivated" };
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);

            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = user,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Error = "Login failed: " + ex.Message };
        }
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return new AuthResult { Success = false, Error = "Passwords do not match" };
            }

            var userExists = await _userService.ExistsAsync(registerDto.Email, registerDto.Username);
            if (userExists)
            {
                return new AuthResult { Success = false, Error = "User with this email or username already exists" };
            }

            var user = await _userService.CreateAsync(registerDto);
            var accessToken = GenerateJwtToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);

            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = user,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Error = "Registration failed: " + ex.Message };
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                return new AuthResult { Success = false, Error = "Invalid or expired refresh token" };
            }

            var user = await _userService.GetByIdAsync(tokenEntity.UserId);
            if (user == null || !user.IsActive)
            {
                return new AuthResult { Success = false, Error = "User not found or inactive" };
            }

            // Revoke the old refresh token
            tokenEntity.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(tokenEntity);

            // Generate new tokens
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);

            return new AuthResult
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = user,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Error = "Token refresh failed: " + ex.Message };
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        try
        {
            var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (tokenEntity == null)
                return false;

            tokenEntity.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(tokenEntity);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateJwtToken(UserDto user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };

        // Add role claims
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Name)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var refreshToken = Convert.ToBase64String(randomBytes);

        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(tokenEntity);
        return refreshToken;
    }
}