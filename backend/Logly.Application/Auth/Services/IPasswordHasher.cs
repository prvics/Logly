using Logly.Domain.Entities;

namespace Logly.Application.Auth.Services;

public interface IPasswordHasher
{
    string Hash(AppUser user, string password);

    bool Verify(
        AppUser user,
        string passwordHash,
        string password);
}