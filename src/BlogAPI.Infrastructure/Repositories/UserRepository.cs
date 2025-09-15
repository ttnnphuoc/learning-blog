using Microsoft.EntityFrameworkCore;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure.Data;

namespace BlogAPI.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(BlogDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> GetWithRolesAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsAsync(string email, string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower() || 
                          u.Username.ToLower() == username.ToLower());
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string roleName)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name.ToLower() == roleName.ToLower()))
            .ToListAsync();
    }
}