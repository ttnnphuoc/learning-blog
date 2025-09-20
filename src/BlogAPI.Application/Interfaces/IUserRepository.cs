using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetWithRolesAsync(Guid id);
    Task<bool> ExistsAsync(string email, string username);
    Task<IEnumerable<User>> GetByRoleAsync(string roleName);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetUsersByRoleAsync(Guid roleId);
    Task AddRoleToUserAsync(Guid userId, Guid roleId);
    Task RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    Task<bool> ValidatePasswordAsync(User user, string password);
    Task UpdatePasswordAsync(User user, string newPassword);
}