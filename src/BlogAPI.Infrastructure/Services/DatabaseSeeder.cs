using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogAPI.Infrastructure.Services;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly BlogDbContext _context;
    private readonly IUserService _userService;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(BlogDbContext context, IUserService userService, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Seed roles first
            await SeedRolesAsync();

            // Seed permissions
            await SeedPermissionsAsync();

            // Assign permissions to roles
            await SeedRolePermissionsAsync();

            // Seed super admin user
            await SeedSuperAdminAsync();

            // Assign admin role to super admin
            await AssignAdminRoleAsync();

            // Update existing posts to have admin author
            await UpdateExistingPostsAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        var defaultRoles = new[]
        {
            new Role { Name = "Admin", Description = "System administrator with full access", IsSystemRole = true },
            new Role { Name = "Author", Description = "Can create and manage own posts", IsSystemRole = true },
            new Role { Name = "Moderator", Description = "Can moderate posts and comments", IsSystemRole = true },
            new Role { Name = "Reader", Description = "Can read published content", IsSystemRole = true }
        };

        foreach (var role in defaultRoles)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role.Name);
            if (existingRole == null)
            {
                _context.Roles.Add(role);
                _logger.LogInformation("Created role: {RoleName}", role.Name);
            }
        }
    }

    private async Task SeedPermissionsAsync()
    {
        var defaultPermissions = new[]
        {
            // Post permissions
            new Permission { Name = "posts.create", Resource = "Posts", Action = "Create", Description = "Can create new posts" },
            new Permission { Name = "posts.read", Resource = "Posts", Action = "Read", Description = "Can read posts" },
            new Permission { Name = "posts.update.own", Resource = "Posts", Action = "UpdateOwn", Description = "Can update own posts" },
            new Permission { Name = "posts.update.any", Resource = "Posts", Action = "UpdateAny", Description = "Can update any posts" },
            new Permission { Name = "posts.delete.own", Resource = "Posts", Action = "DeleteOwn", Description = "Can delete own posts" },
            new Permission { Name = "posts.delete.any", Resource = "Posts", Action = "DeleteAny", Description = "Can delete any posts" },
            new Permission { Name = "posts.publish", Resource = "Posts", Action = "Publish", Description = "Can publish/unpublish posts" },
            
            // User permissions
            new Permission { Name = "users.create", Resource = "Users", Action = "Create", Description = "Can create new users" },
            new Permission { Name = "users.read", Resource = "Users", Action = "Read", Description = "Can view user information" },
            new Permission { Name = "users.update.own", Resource = "Users", Action = "UpdateOwn", Description = "Can update own profile" },
            new Permission { Name = "users.update.any", Resource = "Users", Action = "UpdateAny", Description = "Can update any user" },
            new Permission { Name = "users.delete", Resource = "Users", Action = "Delete", Description = "Can delete users" },
            new Permission { Name = "users.manage.roles", Resource = "Users", Action = "ManageRoles", Description = "Can assign/remove user roles" },
            
            // Category permissions
            new Permission { Name = "categories.create", Resource = "Categories", Action = "Create", Description = "Can create categories" },
            new Permission { Name = "categories.update", Resource = "Categories", Action = "Update", Description = "Can update categories" },
            new Permission { Name = "categories.delete", Resource = "Categories", Action = "Delete", Description = "Can delete categories" },
            
            // Tag permissions
            new Permission { Name = "tags.create", Resource = "Tags", Action = "Create", Description = "Can create tags" },
            new Permission { Name = "tags.update", Resource = "Tags", Action = "Update", Description = "Can update tags" },
            new Permission { Name = "tags.delete", Resource = "Tags", Action = "Delete", Description = "Can delete tags" }
        };

        foreach (var permission in defaultPermissions)
        {
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Resource == permission.Resource && p.Action == permission.Action);
            
            if (existingPermission == null)
            {
                _context.Permissions.Add(permission);
                _logger.LogInformation("Created permission: {PermissionName}", permission.Name);
            }
        }
    }

    private async Task SeedRolePermissionsAsync()
    {
        // Admin gets all permissions
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var allPermissions = await _context.Permissions.ToListAsync();
        
        if (adminRole != null)
        {
            foreach (var permission in allPermissions)
            {
                var existingRolePermission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == permission.Id);
                
                if (existingRolePermission == null)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }

        // Author permissions
        var authorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Author");
        var authorPermissions = new[] { "posts.create", "posts.read", "posts.update.own", "posts.delete.own", "users.update.own" };
        
        if (authorRole != null)
        {
            foreach (var permissionName in authorPermissions)
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
                if (permission != null)
                {
                    var existingRolePermission = await _context.RolePermissions
                        .FirstOrDefaultAsync(rp => rp.RoleId == authorRole.Id && rp.PermissionId == permission.Id);
                    
                    if (existingRolePermission == null)
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = authorRole.Id,
                            PermissionId = permission.Id
                        });
                    }
                }
            }
        }

        // Moderator permissions
        var moderatorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Moderator");
        var moderatorPermissions = new[] { "posts.create", "posts.read", "posts.update.any", "posts.delete.any", "posts.publish", "users.read", "users.update.own", "categories.create", "categories.update", "categories.delete", "tags.create", "tags.update", "tags.delete" };
        
        if (moderatorRole != null)
        {
            foreach (var permissionName in moderatorPermissions)
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
                if (permission != null)
                {
                    var existingRolePermission = await _context.RolePermissions
                        .FirstOrDefaultAsync(rp => rp.RoleId == moderatorRole.Id && rp.PermissionId == permission.Id);
                    
                    if (existingRolePermission == null)
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = moderatorRole.Id,
                            PermissionId = permission.Id
                        });
                    }
                }
            }
        }

        // Reader permissions
        var readerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Reader");
        var readerPermissions = new[] { "posts.read", "users.update.own" };
        
        if (readerRole != null)
        {
            foreach (var permissionName in readerPermissions)
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
                if (permission != null)
                {
                    var existingRolePermission = await _context.RolePermissions
                        .FirstOrDefaultAsync(rp => rp.RoleId == readerRole.Id && rp.PermissionId == permission.Id);
                    
                    if (existingRolePermission == null)
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = readerRole.Id,
                            PermissionId = permission.Id
                        });
                    }
                }
            }
        }
    }

    private async Task SeedSuperAdminAsync()
    {
        const string adminEmail = "admin@blogapi.com";
        const string adminUsername = "admin";

        var existingAdmin = await _userService.GetByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = await _userService.CreateAsync(new RegisterDto
            {
                Username = adminUsername,
                Email = adminEmail,
                Password = "Admin123!@#",
                ConfirmPassword = "Admin123!@#",
                FirstName = "Super",
                LastName = "Admin"
            });

            _logger.LogInformation("Created super admin user: {Email}", adminEmail);
        }
        else
        {
            _logger.LogInformation("Super admin user already exists: {Email}", adminEmail);
        }
    }

    private async Task AssignAdminRoleAsync()
    {
        const string adminEmail = "admin@blogapi.com";
        
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        
        if (adminUser != null && adminRole != null)
        {
            var existingUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
            
            if (existingUserRole == null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                
                _logger.LogInformation("Assigned Admin role to super admin user");
            }
        }
    }

    private async Task UpdateExistingPostsAsync()
    {
        const string adminEmail = "admin@blogapi.com";
        
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (adminUser != null)
        {
            var postsWithoutAuthor = await _context.Posts
                .Where(p => p.AuthorId == Guid.Empty)
                .ToListAsync();
            
            if (postsWithoutAuthor.Any())
            {
                foreach (var post in postsWithoutAuthor)
                {
                    post.AuthorId = adminUser.Id;
                    post.UpdatedAt = DateTime.UtcNow;
                }
                
                _logger.LogInformation("Updated {Count} existing posts to have admin as author", postsWithoutAuthor.Count);
            }
        }
    }
}