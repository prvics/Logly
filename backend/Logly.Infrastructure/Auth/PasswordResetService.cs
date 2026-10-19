using System.Security.Cryptography;
using System.Text;
using Logly.Application.Auth.DTOs;
using Logly.Application.Auth.Services;
using Logly.Application.Email;
using Logly.Domain.Entities;
using Logly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logly.Infrastructure.Auth;

public sealed class PasswordResetService : IPasswordResetService
{
    private readonly LoglyDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;

    public PasswordResetService(
        LoglyDbContext dbContext,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
    }

    public async Task RequestResetAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant()
                    ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.Email == email && user.IsActive,
                cancellationToken);

        if (user is null)
        {
            return;
        }

        var token = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));

        var now = DateTime.UtcNow;

        var resetToken = new PasswordResetToken
        {
            AppUserId = user.Id,
            TokenHash = HashToken(token),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(15)
        };

        _dbContext.PasswordResetTokens.Add(resetToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var body =
            "Szia!\n\n" +
            "Jelszó-helyreállítást kértél a Logly-fiókodhoz.\n\n" +
            "A helyreállító tokened:\n\n" +
            token +
            "\n\nA token 15 percig érvényes, és egyszer használható." +
            "\n\nHa nem te kérted a helyreállítást, " +
            "hagyd figyelmen kívül ezt az üzenetet.";

        await _emailSender.SendAsync(
            user.Email,
            "Logly – Jelszó-helyreállítás",
            body,
            cancellationToken);
    }

    public async Task<bool> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token) ||
            request.Token.Length != 64 ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            request.NewPassword.Length < 12)
        {
            return false;
        }

        var normalizedToken = request.Token.ToUpperInvariant();
        var tokenHash = HashToken(normalizedToken);
        var now = DateTime.UtcNow;

        var resetToken = await _dbContext.PasswordResetTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(
                token =>
                    token.TokenHash == tokenHash &&
                    token.UsedAtUtc == null &&
                    token.ExpiresAtUtc > now,
                cancellationToken);

        if (resetToken is null)
        {
            return false;
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var claimedTokens = await _dbContext.PasswordResetTokens
            .Where(token =>
                token.Id == resetToken.Id &&
                token.UsedAtUtc == null &&
                token.ExpiresAtUtc > now)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    token => token.UsedAtUtc,
                    (DateTime?)now),
                cancellationToken);

        if (claimedTokens != 1)
        {
            return false;
        }

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                user =>
                    user.Id == resetToken.AppUserId &&
                    user.IsActive,
                cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.Hash(
            user,
            request.NewPassword);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _dbContext.PasswordResetTokens
            .Where(token =>
                token.AppUserId == user.Id &&
                token.UsedAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    token => token.UsedAtUtc,
                    (DateTime?)now),
                cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        return Convert.ToHexString(
            SHA256.HashData(bytes));
    }
}