using FluentValidation;
using Logly.Application.Auth.DTOs;

namespace Logly.Application.Auth.Validators;

public sealed class LoginRequestValidator
    : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Az email cím megadása kötelező.")
            .EmailAddress()
            .WithMessage("Az email cím formátuma érvénytelen.");

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage("A jelszó megadása kötelező.");
    }
}