using FluentValidation;
using Logly.Application.Vehicles.DTOs;

namespace Logly.Application.Vehicles.Validators;

public sealed class CreateVehicleRequestValidator
    : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("A jármű neve kötelező.")
            .MaximumLength(100)
            .WithMessage("A jármű neve legfeljebb 100 karakter lehet.");

        RuleFor(request => request.Type)
            .IsInEnum()
            .WithMessage("A jármű típusa érvénytelen.");

        RuleFor(request => request.StartingOdometerKm)
            .GreaterThanOrEqualTo(0)
            .When(request => request.StartingOdometerKm.HasValue)
            .WithMessage("A kezdő kilométeróra állás nem lehet negatív.");
    }
}