using Logly.Application.Auth.Services;
using Logly.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Logly.Infrastructure.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public string Hash(AppUser user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool Verify(
        AppUser user,
        string passwordHash,
        string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            passwordHash,
            password);

        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}