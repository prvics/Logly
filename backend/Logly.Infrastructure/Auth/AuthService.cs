using Logly.Application.Auth.DTOs;
using Logly.Application.Auth.Repositories;
using Logly.Application.Auth.Services;
using Logly.Domain.Entities;

namespace Logly.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IAppUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IAppUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse?> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            request.Password.Length < 12)
        {
            return null;
        }

        var emailAlreadyExists = await _userRepository.EmailExistsAsync(
            email,
            cancellationToken);

        if (emailAlreadyExists)
        {
            return null;
        }

        var user = new AppUser
        {
            Email = email,
            DisplayName = NormalizeDisplayName(request.DisplayName)
        };

        user.PasswordHash = _passwordHasher.Hash(
            user,
            request.Password);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _jwtTokenGenerator.Create(user);
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await _userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !_passwordHasher.Verify(
                user,
                user.PasswordHash,
                request.Password))
        {
            return null;
        }

        return _jwtTokenGenerator.Create(user);
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        return new CurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.IsAdmin ? "Admin" : "User",
            CreatedAtUtc = user.CreatedAtUtc
        };
    }

    private static string NormalizeEmail(string? email)
    {
        return email?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static string? NormalizeDisplayName(string? displayName)
    {
        return string.IsNullOrWhiteSpace(displayName)
            ? null
            : displayName.Trim();
    }
}