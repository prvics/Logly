using FluentValidation;
using Logly.Application.Expenses.DTOs;
using Logly.Domain.Enums;

namespace Logly.Application.Expenses.Validators;

public sealed class CreateExpenseRequestValidator
    : AbstractValidator<CreateExpenseRequest>
{
    public CreateExpenseRequestValidator()
    {
        RuleFor(request => request.Date)
            .NotEqual(default(DateOnly))
            .WithMessage("A kiadás dátuma kötelező.");

        RuleFor(request => request.Amount)
            .GreaterThan(0)
            .WithMessage("A kiadás összege nagyobb kell legyen nullánál.");

        RuleFor(request => request.Category)
            .IsInEnum()
            .WithMessage("A kiadás kategóriája érvénytelen.");

        RuleFor(request => request.VehicleId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("A jármű azonosítója nem lehet üres.");

        RuleFor(request => request.UserCompanyId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("A futárcég azonosítója nem lehet üres.");

        RuleFor(request => request.Note)
            .MaximumLength(500)
            .WithMessage("A megjegyzés legfeljebb 500 karakter lehet.");
    }
}