using FluentValidation;
using Logly.Application.Auth.DTOs;

namespace Logly.Application.Auth.Validators;

public sealed class ResetPasswordRequestValidator
    : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.Token)
            .NotEmpty()
            .WithMessage("A helyreállító token megadása kötelező.")
            .Matches("^[A-Fa-f0-9]{64}$")
            .WithMessage("A helyreállító token formátuma érvénytelen.");

        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .WithMessage("Az új jelszó megadása kötelező.")
            .MinimumLength(12)
            .WithMessage(
                "Az új jelszónak legalább 12 karakterből kell állnia.");
    }
}