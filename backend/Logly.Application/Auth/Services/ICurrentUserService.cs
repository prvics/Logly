namespace Logly.Application.Auth.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }

    bool IsAdmin { get; }
}