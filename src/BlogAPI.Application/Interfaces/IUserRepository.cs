using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetWithRolesAsync(Guid id);
    Task<bool> ExistsAsync(string email, string username);
    Task<IEnumerable<User>> GetByRoleAsync(string roleName);
}