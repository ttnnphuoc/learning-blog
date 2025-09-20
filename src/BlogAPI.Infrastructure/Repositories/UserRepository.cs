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
            .Where(u => !u.IsDeleted)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> GetWithRolesAsync(Guid id)
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsAsync(string email, string username)
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .AnyAsync(u => u.Email.ToLower() == email.ToLower() || 
                          u.Username.ToLower() == username.ToLower());
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string roleName)
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name.ToLower() == roleName.ToLower()))
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => !u.IsDeleted && u.IsActive)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(Guid roleId)
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
            .ToListAsync();
    }

    public async Task AddRoleToUserAsync(Guid userId, Guid roleId)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ValidatePasswordAsync(User user, string password)
    {
        // This would typically use a password hashing library like BCrypt
        // For now, we'll do a simple hash comparison
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task UpdatePasswordAsync(User user, string newPassword)
    {
        // Hash the new password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}