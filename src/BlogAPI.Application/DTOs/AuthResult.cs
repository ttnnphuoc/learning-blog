namespace BlogAPI.Application.DTOs;

public class AuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public UserDto? User { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Error { get; set; }
}