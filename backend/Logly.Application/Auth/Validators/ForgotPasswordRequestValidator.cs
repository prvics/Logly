using FluentValidation;
using Logly.Application.Auth.DTOs;

namespace Logly.Application.Auth.Validators;

public sealed class ForgotPasswordRequestValidator
    : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Az email cím megadása kötelező.")
            .EmailAddress()
            .WithMessage("Az email cím formátuma érvénytelen.");
    }
}