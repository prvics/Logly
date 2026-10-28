using System.Security.Claims;
using Logly.Application.Auth.Services;

namespace Logly.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException(
                    "Nincs bejelentkezett felhasználó.");
            }

            var value =
                principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue("sub");

            if (!Guid.TryParse(value, out var userId) ||
                userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "Érvénytelen felhasználói azonosító.");
            }

            return userId;
        }
    }

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext?.User
            .IsInRole("Admin") == true;
}