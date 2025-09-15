using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(loginDto);
        
        if (!result.Success)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(registerDto);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
        
        if (!result.Success)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result);
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<ActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _authService.RevokeTokenAsync(refreshTokenDto.RefreshToken);
        
        if (!success)
        {
            return BadRequest(new { error = "Failed to revoke token" });
        }

        return Ok(new { message = "Token revoked successfully" });
    }

    [HttpPost("validate")]
    [Authorize]
    public async Task<ActionResult> ValidateToken()
    {
        // If we reach here, the token is valid (due to [Authorize] attribute)
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        var username = User.Identity?.Name;
        
        return Ok(new 
        { 
            valid = true, 
            userId = userId,
            username = username,
            message = "Token is valid" 
        });
    }
}