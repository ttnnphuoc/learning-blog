# Authentication and Authorization Feature Specification

## Overview

This document outlines the implementation plan for adding comprehensive authentication and authorization to the Blog API. This feature will transform the API from a publicly accessible service to a secure, multi-user blogging platform.

## Current State

- **No authentication**: All API endpoints are publicly accessible
- **No user management**: No concept of users, roles, or permissions
- **No data ownership**: Posts can be modified by anyone
- **Security vulnerability**: Potential for unauthorized access and data manipulation

## Feature Goals

### Primary Objectives
1. **Secure the API**: Protect sensitive operations behind authentication
2. **Multi-user support**: Enable multiple users to manage their own content
3. **Role-based access**: Implement different permission levels
4. **Data ownership**: Ensure users can only access/modify their own content
5. **Scalable security**: Design for future permission expansions

### Secondary Objectives
- Maintain Clean Architecture principles
- Ensure backward compatibility where possible
- Implement industry-standard security practices
- Support for future features (user profiles, social features)

## Technical Requirements

### Authentication System
- **JWT Token-based**: Stateless authentication using JSON Web Tokens
- **Token expiration**: Configurable token lifetime (default: 24 hours)
- **Refresh tokens**: Secure token renewal mechanism
- **Password security**: BCrypt hashing with salt rounds
- **Account lockout**: Protection against brute force attacks

### Authorization System
- **Role-based access control (RBAC)**: Users assigned to roles with specific permissions
- **Resource ownership**: Users can only modify their own content
- **Permission granularity**: Fine-grained control over operations
- **Policy-based authorization**: Flexible authorization rules

## Domain Model Changes

### New Entities

#### User Entity
```csharp
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    
    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
```

#### Role Entity
```csharp
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; } // Cannot be deleted
    
    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
```

#### Permission Entity
```csharp
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // Posts, Categories, Users, etc.
    public string Action { get; set; } = string.Empty; // Create, Read, Update, Delete
    public string Description { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
```

#### UserRole (Junction Table)
```csharp
public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
```

#### RolePermission (Junction Table)
```csharp
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    
    // Navigation properties
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
```

#### RefreshToken Entity
```csharp
public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public Guid UserId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}
```

### Modified Entities

#### Post Entity Updates
```csharp
public class Post : BaseEntity
{
    // Existing properties...
    
    // New properties
    public Guid AuthorId { get; set; } // Foreign key to User
    
    // Navigation properties
    public User Author { get; set; } = null!;
}
```

## Application Layer Changes

### New Services

#### IAuthService
```csharp
public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginDto loginDto);
    Task<AuthResult> RegisterAsync(RegisterDto registerDto);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<bool> ConfirmEmailAsync(Guid userId, string token);
}
```

#### IUserService
```csharp
public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto> CreateAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateUserDto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> AssignRoleAsync(Guid userId, Guid roleId);
    Task<bool> RemoveRoleAsync(Guid userId, Guid roleId);
    Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId);
}
```

#### IRoleService
```csharp
public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<RoleDto?> GetByIdAsync(Guid id);
    Task<RoleDto> CreateAsync(CreateRoleDto createRoleDto);
    Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto updateRoleDto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> AssignPermissionAsync(Guid roleId, Guid permissionId);
    Task<bool> RemovePermissionAsync(Guid roleId, Guid permissionId);
}
```

### New DTOs

#### Authentication DTOs
```csharp
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class RegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public UserDto? User { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Error { get; set; }
}
```

#### User DTOs
```csharp
public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<RoleDto> Roles { get; set; } = new List<RoleDto>();
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<Guid> RoleIds { get; set; } = new List<Guid>();
}
```

## Infrastructure Layer Changes

### New Repositories

#### IUserRepository
```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetWithRolesAsync(Guid id);
    Task<bool> ExistsAsync(string email, string username);
    Task<IEnumerable<User>> GetByRoleAsync(string roleName);
}
```

#### IRoleRepository
```csharp
public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> GetWithPermissionsAsync(Guid id);
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
}
```

#### IRefreshTokenRepository
```csharp
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
    Task RevokeAllUserTokensAsync(Guid userId);
    Task CleanupExpiredTokensAsync();
}
```

### Database Context Updates

```csharp
public class BlogDbContext : DbContext
{
    // Existing DbSets...
    
    // New DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure relationships and constraints
        ConfigureUserEntity(modelBuilder);
        ConfigureRoleEntity(modelBuilder);
        ConfigurePermissionEntity(modelBuilder);
        ConfigureJunctionTables(modelBuilder);
        ConfigureRefreshTokenEntity(modelBuilder);
        
        // Seed default data
        SeedDefaultRolesAndPermissions(modelBuilder);
    }
}
```

## Presentation Layer Changes

### New Controllers

#### AuthController
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginDto loginDto);
    
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterDto registerDto);
    
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto);
    
    [HttpPost("revoke")]
    [Authorize]
    public async Task<ActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto);
    
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto);
}
```

#### UsersController
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers();
    
    [HttpGet("{id}")]
    [Authorize(Policy = "OwnerOrAdmin")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id);
    
    [HttpPut("{id}")]
    [Authorize(Policy = "OwnerOrAdmin")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto);
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteUser(Guid id);
}
```

### Updated Controllers

#### PostsController Updates
```csharp
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts(); // Public
    
    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetPost(Guid id); // Public for published posts
    
    [HttpPost]
    [Authorize(Roles = "Author,Admin")]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto createPostDto);
    
    [HttpPut("{id}")]
    [Authorize(Policy = "PostOwnerOrAdmin")]
    public async Task<ActionResult<PostDto>> UpdatePost(Guid id, [FromBody] UpdatePostDto updatePostDto);
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "PostOwnerOrAdmin")]
    public async Task<ActionResult> DeletePost(Guid id);
}
```

## Security Configuration

### JWT Configuration
```csharp
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 1440; // 24 hours
    public int RefreshTokenExpirationDays { get; set; } = 30;
}
```

### Authorization Policies
```csharp
public static class AuthorizationPolicies
{
    public const string OwnerOrAdmin = "OwnerOrAdmin";
    public const string PostOwnerOrAdmin = "PostOwnerOrAdmin";
    public const string AdminOnly = "AdminOnly";
}
```

## Default Roles and Permissions

### System Roles
1. **Admin**: Full system access
2. **Author**: Can create and manage own posts
3. **Moderator**: Can manage posts and comments
4. **Reader**: Can view published content

### Permissions Matrix
| Resource | Action | Admin | Author | Moderator | Reader |
|----------|--------|--------|--------|-----------|--------|
| Posts | Create | ✓ | ✓ | ✓ | ✗ |
| Posts | Read | ✓ | ✓ | ✓ | ✓ |
| Posts | Update Own | ✓ | ✓ | ✓ | ✗ |
| Posts | Update Any | ✓ | ✗ | ✓ | ✗ |
| Posts | Delete Own | ✓ | ✓ | ✓ | ✗ |
| Posts | Delete Any | ✓ | ✗ | ✓ | ✗ |
| Users | Manage | ✓ | ✗ | ✗ | ✗ |
| Categories | Manage | ✓ | ✗ | ✓ | ✗ |

## Implementation Plan

### Phase 1: Core Authentication (Week 1-2)
1. Create domain entities and database migrations
2. Implement basic authentication services
3. Add JWT middleware and configuration
4. Create login/register endpoints
5. Update existing endpoints with basic authorization

### Phase 2: Authorization System (Week 3-4)
1. Implement role-based authorization
2. Create user management endpoints
3. Add permission-based policies
4. Update all controllers with proper authorization
5. Implement resource ownership validation

### Phase 3: Security Enhancements (Week 5-6)
1. Add refresh token mechanism
2. Implement password reset functionality
3. Add email confirmation
4. Implement account lockout protection
5. Add comprehensive logging and monitoring

### Phase 4: Testing and Documentation (Week 7-8)
1. Write comprehensive unit tests
2. Add integration tests for authentication flows
3. Update API documentation
4. Create user guides and examples
5. Performance testing and optimization

## Security Considerations

### Best Practices
- **Password Security**: BCrypt with salt rounds >= 12
- **JWT Security**: Strong secret key, appropriate expiration
- **Input Validation**: Comprehensive validation on all inputs
- **Rate Limiting**: Protect against brute force attacks
- **HTTPS Only**: Enforce secure connections in production
- **Audit Logging**: Log all security-related events

### Vulnerabilities to Address
- SQL Injection (through parameterized queries)
- Cross-Site Request Forgery (CSRF tokens)
- Cross-Site Scripting (XSS prevention)
- Sensitive data exposure (no passwords in responses)
- Broken authentication (secure session management)

## Configuration Updates

### appsettings.json
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-jwt-key-here",
    "Issuer": "BlogAPI",
    "Audience": "BlogAPI-Users",
    "AccessTokenExpirationMinutes": 1440,
    "RefreshTokenExpirationDays": 30
  },
  "PasswordSettings": {
    "RequiredLength": 8,
    "RequireDigit": true,
    "RequireLowercase": true,
    "RequireUppercase": true,
    "RequireNonAlphanumeric": true,
    "MaxFailedAttempts": 5,
    "LockoutDurationMinutes": 30
  }
}
```

## Testing Strategy

### Unit Tests
- Authentication service logic
- Authorization policy evaluation
- Password hashing and validation
- Token generation and validation
- User management operations

### Integration Tests
- Complete authentication flows
- Authorization on protected endpoints
- Token refresh mechanisms
- Database operations with security context
- API endpoint security

### Security Tests
- Penetration testing for common vulnerabilities
- Load testing for authentication endpoints
- Token manipulation and validation tests
- Privilege escalation prevention tests

## Migration Strategy

### Database Migrations
1. Create new authentication tables
2. Add user foreign keys to existing tables
3. Migrate existing data (assign default author)
4. Add constraints and indexes

### API Compatibility
- Maintain public read endpoints
- Gradually introduce authentication requirements
- Provide clear migration documentation
- Support for API versioning

## Success Criteria

### Functional Requirements
- [ ] Users can register and login successfully
- [ ] JWT tokens are properly generated and validated
- [ ] Role-based access control works correctly
- [ ] Resource ownership is properly enforced
- [ ] Password security meets industry standards

### Non-Functional Requirements
- [ ] Authentication adds < 100ms latency
- [ ] System supports 1000+ concurrent users
- [ ] 99.9% uptime for authentication services
- [ ] Comprehensive audit logging
- [ ] Security vulnerability assessment passes

## Future Enhancements

### Social Authentication
- OAuth2 integration (Google, GitHub, etc.)
- Social media login options
- External identity provider support

### Advanced Security
- Multi-factor authentication (MFA)
- Single Sign-On (SSO) support
- Advanced threat detection
- Behavioral analytics

### User Experience
- Email verification workflows
- Password strength indicators
- Account recovery options
- User preference management

---

**Document Version**: 1.0  
**Last Updated**: 2025-09-15  
**Status**: Draft  
**Owner**: Development Team