using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
    Task RevokeAllUserTokensAsync(Guid userId);
    Task CleanupExpiredTokensAsync();
}