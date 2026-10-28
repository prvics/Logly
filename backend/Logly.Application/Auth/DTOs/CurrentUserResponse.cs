namespace Logly.Application.Auth.DTOs;

public sealed class CurrentUserResponse
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string? DisplayName { get; init; }
    
    public string Role { get; init; } = "User";

    public DateTime CreatedAtUtc { get; init; }
}