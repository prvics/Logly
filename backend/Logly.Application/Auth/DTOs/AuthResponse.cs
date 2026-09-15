namespace Logly.Application.Auth.DTOs;

public sealed class AuthResponse
{
    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string? DisplayName { get; init; }

    public string AccessToken { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }
}