using Logly.Application.Auth.DTOs;

namespace Logly.Application.Auth.Services;

public interface IPasswordResetService
{
    Task RequestResetAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken);

    Task<bool> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken);
}