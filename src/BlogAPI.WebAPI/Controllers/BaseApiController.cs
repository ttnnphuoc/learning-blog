using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogAPI.WebAPI.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController(ILogger logger) : ControllerBase
{
    protected readonly ILogger Logger = logger;

    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    /// <returns>Current user ID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user ID cannot be extracted</exception>
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            Logger.LogWarning("Invalid user token or missing NameIdentifier claim");
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    /// <summary>
    /// Checks if the current user has admin privileges
    /// </summary>
    /// <returns>True if user is admin, false otherwise</returns>
    protected bool IsCurrentUserAdmin()
    {
        return User.IsInRole("Admin") || User.HasClaim("role", "Admin");
    }

    /// <summary>
    /// Checks if the current user has moderator privileges
    /// </summary>
    /// <returns>True if user is moderator, false otherwise</returns>
    protected bool IsCurrentUserModerator()
    {
        return User.IsInRole("Moderator") || User.HasClaim("role", "Moderator");
    }

    /// <summary>
    /// Checks if the current user has admin or moderator privileges
    /// </summary>
    /// <returns>True if user has admin access, false otherwise</returns>
    protected bool HasAdminAccess()
    {
        return IsCurrentUserAdmin() || IsCurrentUserModerator();
    }

    /// <summary>
    /// Gets current user authorization info (ID and admin status)
    /// </summary>
    /// <returns>Tuple containing user ID and admin status</returns>
    protected (Guid UserId, bool IsAdmin) GetCurrentUserInfo()
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();
        return (userId, isAdmin);
    }

    /// <summary>
    /// Gets current user authorization info with admin access check
    /// </summary>
    /// <returns>Tuple containing user ID and admin access status</returns>
    protected (Guid UserId, bool HasAdminAccess) GetCurrentUserInfoWithAdminAccess()
    {
        var userId = GetCurrentUserId();
        var hasAdminAccess = HasAdminAccess();
        return (userId, hasAdminAccess);
    }

    /// <summary>
    /// Gets the current user's username
    /// </summary>
    /// <returns>Current user's username</returns>
    protected string GetCurrentUsername()
    {
        var usernameClaim = User.FindFirst(ClaimTypes.Name) ?? User.FindFirst("username");
        return usernameClaim?.Value ?? "Unknown";
    }

    /// <summary>
    /// Gets the current user's email
    /// </summary>
    /// <returns>Current user's email</returns>
    protected string? GetCurrentUserEmail()
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email) ?? User.FindFirst("email");
        return emailClaim?.Value;
    }

    /// <summary>
    /// Gets all roles for the current user
    /// </summary>
    /// <returns>List of user roles</returns>
    protected IEnumerable<string> GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Concat(User.FindAll("role").Select(c => c.Value))
            .Distinct();
    }

    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    /// <param name="role">Role to check</param>
    /// <returns>True if user has the role, false otherwise</returns>
    protected bool HasRole(string role)
    {
        return User.IsInRole(role) || User.HasClaim("role", role);
    }

    /// <summary>
    /// Checks if the current user has any of the specified roles
    /// </summary>
    /// <param name="roles">Roles to check</param>
    /// <returns>True if user has any of the roles, false otherwise</returns>
    protected bool HasAnyRole(params string[] roles)
    {
        return roles.Any(HasRole);
    }

    /// <summary>
    /// Handles common authorization exceptions and returns appropriate HTTP responses
    /// </summary>
    /// <param name="ex">The exception to handle</param>
    /// <param name="operation">The operation being performed (for logging)</param>
    /// <returns>Appropriate HTTP response</returns>
    protected IActionResult HandleAuthorizationException(UnauthorizedAccessException ex, string operation)
    {
        Logger.LogWarning("Unauthorized attempt to {Operation}: {Message}", operation, ex.Message);
        return Forbid($"You are not authorized to {operation}");
    }

    /// <summary>
    /// Handles common authorization exceptions and returns appropriate HTTP responses for generic ActionResult
    /// </summary>
    /// <typeparam name="T">The expected return type</typeparam>
    /// <param name="ex">The exception to handle</param>
    /// <param name="operation">The operation being performed (for logging)</param>
    /// <returns>Appropriate HTTP response</returns>
    protected ActionResult<T> HandleAuthorizationException<T>(UnauthorizedAccessException ex, string operation)
    {
        Logger.LogWarning("Unauthorized attempt to {Operation}: {Message}", operation, ex.Message);
        return Forbid($"You are not authorized to {operation}");
    }

    /// <summary>
    /// Logs user action for auditing purposes
    /// </summary>
    /// <param name="action">Action being performed</param>
    /// <param name="resourceId">ID of the resource being acted upon</param>
    /// <param name="additionalInfo">Additional information for logging</param>
    protected void LogUserAction(string action, Guid? resourceId = null, object? additionalInfo = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var username = GetCurrentUsername();
            
            if (resourceId.HasValue)
            {
                Logger.LogInformation("User {Username} ({UserId}) performed {Action} on resource {ResourceId}. Additional info: {AdditionalInfo}", 
                    username, userId, action, resourceId.Value, additionalInfo);
            }
            else
            {
                Logger.LogInformation("User {Username} ({UserId}) performed {Action}. Additional info: {AdditionalInfo}", 
                    username, userId, action, additionalInfo);
            }
        }
        catch (UnauthorizedAccessException)
        {
            Logger.LogWarning("Attempted to log action {Action} for unauthorized user", action);
        }
    }
}