using FluentValidation;
using Logly.Application.Auth.DTOs;

namespace Logly.Application.Auth.Validators;

public sealed class RegisterRequestValidator
    : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Az email cím megadása kötelező.")
            .EmailAddress()
            .WithMessage("Az email cím formátuma érvénytelen.");

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage("A jelszó megadása kötelező.")
            .MinimumLength(8)
            .WithMessage("A jelszónak legalább 8 karakterből kell állnia.");

        RuleFor(request => request.DisplayName)
            .NotEmpty()
            .WithMessage("A megjelenített név nem állhat csak szóközökből.")
            .MaximumLength(100)
            .WithMessage("A megjelenített név legfeljebb 100 karakter lehet.");
    }
}