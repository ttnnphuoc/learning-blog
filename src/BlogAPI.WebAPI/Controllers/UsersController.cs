using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Entities;

namespace BlogAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository userRepository, IRoleRepository roleRepository, ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Bio = u.Bio,
                IsEmailConfirmed = u.IsEmailConfirmed,
                LastLoginAt = u.LastLoginAt,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                Roles = u.UserRoles?.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsSystemRole = ur.Role.IsSystemRole,
                    CreatedAt = ur.Role.CreatedAt,
                    UpdatedAt = ur.Role.UpdatedAt
                }) ?? new List<RoleDto>()
            });

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        try
        {
            var user = await _userRepository.GetWithRolesAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                IsEmailConfirmed = user.IsEmailConfirmed,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles?.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsSystemRole = ur.Role.IsSystemRole,
                    CreatedAt = ur.Role.CreatedAt,
                    UpdatedAt = ur.Role.UpdatedAt
                }) ?? new List<RoleDto>()
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Get current authenticated user
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                             User.FindFirst("sub")?.Value ?? 
                             User.FindFirst("id")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var user = await _userRepository.GetWithRolesAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                IsEmailConfirmed = user.IsEmailConfirmed,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles?.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsSystemRole = ur.Role.IsSystemRole,
                    CreatedAt = ur.Role.CreatedAt,
                    UpdatedAt = ur.Role.UpdatedAt
                }) ?? new List<RoleDto>()
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return StatusCode(500, "An error occurred while retrieving the current user");
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(createUserDto.Username))
            {
                return BadRequest("Username is required");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.Email))
            {
                return BadRequest("Email is required");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.Password))
            {
                return BadRequest("Password is required");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.FirstName))
            {
                return BadRequest("First name is required");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.LastName))
            {
                return BadRequest("Last name is required");
            }

            // Check if user already exists
            var existingUserByEmail = await _userRepository.GetByEmailAsync(createUserDto.Email);
            if (existingUserByEmail != null)
            {
                return Conflict($"A user with email '{createUserDto.Email}' already exists");
            }

            var existingUserByUsername = await _userRepository.GetByUsernameAsync(createUserDto.Username);
            if (existingUserByUsername != null)
            {
                return Conflict($"A user with username '{createUserDto.Username}' already exists");
            }

            var user = new User
            {
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Bio = createUserDto.Bio,
                IsActive = true,
                IsEmailConfirmed = false
            };

            var createdUser = await _userRepository.AddAsync(user);

            // Set password
            await _userRepository.UpdatePasswordAsync(createdUser, createUserDto.Password);

            // Add roles if provided
            if (createUserDto.RoleIds?.Any() == true)
            {
                foreach (var roleId in createUserDto.RoleIds)
                {
                    var role = await _roleRepository.GetByIdAsync(roleId);
                    if (role != null)
                    {
                        await _userRepository.AddRoleToUserAsync(createdUser.Id, roleId);
                    }
                }
            }

            // Get the user with roles for response
            var userWithRoles = await _userRepository.GetWithRolesAsync(createdUser.Id);

            var userDto = new UserDto
            {
                Id = userWithRoles!.Id,
                Username = userWithRoles.Username,
                Email = userWithRoles.Email,
                FirstName = userWithRoles.FirstName,
                LastName = userWithRoles.LastName,
                Bio = userWithRoles.Bio,
                IsEmailConfirmed = userWithRoles.IsEmailConfirmed,
                LastLoginAt = userWithRoles.LastLoginAt,
                IsActive = userWithRoles.IsActive,
                CreatedAt = userWithRoles.CreatedAt,
                UpdatedAt = userWithRoles.UpdatedAt,
                Roles = userWithRoles.UserRoles?.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsSystemRole = ur.Role.IsSystemRole,
                    CreatedAt = ur.Role.CreatedAt,
                    UpdatedAt = ur.Role.UpdatedAt
                }) ?? new List<RoleDto>()
            };

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserDto updateUserDto)
    {
        try
        {
            var user = await _userRepository.GetWithRolesAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(updateUserDto.Username))
            {
                // Check if new username already exists (and it's not the current user)
                var existingUser = await _userRepository.GetByUsernameAsync(updateUserDto.Username);
                if (existingUser != null && existingUser.Id != id)
                {
                    return Conflict($"A user with username '{updateUserDto.Username}' already exists");
                }
                user.Username = updateUserDto.Username;
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
            {
                // Check if new email already exists (and it's not the current user)
                var existingUser = await _userRepository.GetByEmailAsync(updateUserDto.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return Conflict($"A user with email '{updateUserDto.Email}' already exists");
                }
                user.Email = updateUserDto.Email;
                user.IsEmailConfirmed = false; // Reset email confirmation when email changes
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
            {
                user.FirstName = updateUserDto.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
            {
                user.LastName = updateUserDto.LastName;
            }

            if (updateUserDto.Bio != null)
            {
                user.Bio = updateUserDto.Bio;
            }

            if (updateUserDto.IsActive.HasValue)
            {
                user.IsActive = updateUserDto.IsActive.Value;
            }

            await _userRepository.UpdateAsync(user);

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
            {
                await _userRepository.UpdatePasswordAsync(user, updateUserDto.Password);
            }

            // Update roles if provided
            if (updateUserDto.RoleIds != null)
            {
                // Remove existing roles
                if (user.UserRoles?.Any() == true)
                {
                    foreach (var userRole in user.UserRoles.ToList())
                    {
                        await _userRepository.RemoveRoleFromUserAsync(id, userRole.RoleId);
                    }
                }

                // Add new roles
                foreach (var roleId in updateUserDto.RoleIds)
                {
                    var role = await _roleRepository.GetByIdAsync(roleId);
                    if (role != null)
                    {
                        await _userRepository.AddRoleToUserAsync(id, roleId);
                    }
                }
            }

            // Get updated user with roles
            var updatedUser = await _userRepository.GetWithRolesAsync(id);

            var userDto = new UserDto
            {
                Id = updatedUser!.Id,
                Username = updatedUser.Username,
                Email = updatedUser.Email,
                FirstName = updatedUser.FirstName,
                LastName = updatedUser.LastName,
                Bio = updatedUser.Bio,
                IsEmailConfirmed = updatedUser.IsEmailConfirmed,
                LastLoginAt = updatedUser.LastLoginAt,
                IsActive = updatedUser.IsActive,
                CreatedAt = updatedUser.CreatedAt,
                UpdatedAt = updatedUser.UpdatedAt,
                Roles = updatedUser.UserRoles?.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsSystemRole = ur.Role.IsSystemRole,
                    CreatedAt = ur.Role.CreatedAt,
                    UpdatedAt = ur.Role.UpdatedAt
                }) ?? new List<RoleDto>()
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user");
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            await _userRepository.SoftDeleteAsync(user);
            return Ok(new { message = "User moved to trash successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user");
        }
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return NotFound($"User with username '{username}' not found");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                IsEmailConfirmed = user.IsEmailConfirmed,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles?.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsSystemRole = ur.Role.IsSystemRole,
                    CreatedAt = ur.Role.CreatedAt,
                    UpdatedAt = ur.Role.UpdatedAt
                }) ?? new List<RoleDto>()
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by username {Username}", username);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    [HttpGet("email/{email}")]
    public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"User with email '{email}' not found");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                IsEmailConfirmed = user.IsEmailConfirmed,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles?.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsSystemRole = ur.Role.IsSystemRole,
                    CreatedAt = ur.Role.CreatedAt,
                    UpdatedAt = ur.Role.UpdatedAt
                }) ?? new List<RoleDto>()
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email {Email}", email);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Get active users
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetActiveUsers()
    {
        try
        {
            var users = await _userRepository.GetActiveUsersAsync();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Bio = u.Bio,
                IsEmailConfirmed = u.IsEmailConfirmed,
                LastLoginAt = u.LastLoginAt,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                Roles = u.UserRoles?.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsSystemRole = ur.Role.IsSystemRole,
                    CreatedAt = ur.Role.CreatedAt,
                    UpdatedAt = ur.Role.UpdatedAt
                }) ?? new List<RoleDto>()
            });

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active users");
            return StatusCode(500, "An error occurred while retrieving active users");
        }
    }

    /// <summary>
    /// Add role to user
    /// </summary>
    [HttpPost("{userId}/roles/{roleId}")]
    public async Task<IActionResult> AddRoleToUser(Guid userId, Guid roleId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found");
            }

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                return NotFound($"Role with ID {roleId} not found");
            }

            await _userRepository.AddRoleToUserAsync(userId, roleId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding role {RoleId} to user {UserId}", roleId, userId);
            return StatusCode(500, "An error occurred while adding role to user");
        }
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    [HttpDelete("{userId}/roles/{roleId}")]
    public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found");
            }

            await _userRepository.RemoveRoleFromUserAsync(userId, roleId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleId} from user {UserId}", roleId, userId);
            return StatusCode(500, "An error occurred while removing role from user");
        }
    }
}