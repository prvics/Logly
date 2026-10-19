namespace Logly.Application.Auth.DTOs;

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}