using FluentValidation;
using Logly.Application.Shifts.DTOs;

namespace Logly.Application.Shifts.Validators;

public sealed class CreateShiftRequestValidator
    : AbstractValidator<CreateShiftRequest>
{
    public CreateShiftRequestValidator()
    {
        RuleFor(request => request.UserCompanyId)
            .NotEmpty()
            .WithMessage("A futárcég kiválasztása kötelező.");

        RuleFor(request => request.VehicleId)
            .NotEmpty()
            .WithMessage("A jármű kiválasztása kötelező.");

        RuleFor(request => request.Date)
            .NotEqual(default(DateOnly))
            .WithMessage("A műszak dátuma kötelező.");

        RuleFor(request => request.WorkedHours)
            .GreaterThanOrEqualTo(0)
            .WithMessage("A ledolgozott óra nem lehet negatív.");

        RuleFor(request => request.WorkedMinutes)
            .InclusiveBetween(0, 59)
            .WithMessage("A ledolgozott perc értéke 0 és 59 közé essen.");

        RuleFor(request => request.DrivenKilometers)
            .GreaterThanOrEqualTo(0)
            .WithMessage("A megtett kilométer nem lehet negatív.");

        RuleFor(request => request.Revenue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("A bevétel nem lehet negatív.");

        RuleFor(request => request.DeliveredOrders)
            .GreaterThanOrEqualTo(0)
            .WithMessage("A rendelések száma nem lehet negatív.");

        RuleFor(request => request.WorkExpense)
            .GreaterThanOrEqualTo(0)
            .WithMessage("A műszakköltség nem lehet negatív.");

        RuleFor(request => request.Note)
            .MaximumLength(500)
            .WithMessage("A megjegyzés legfeljebb 500 karakter lehet.");
    }
}