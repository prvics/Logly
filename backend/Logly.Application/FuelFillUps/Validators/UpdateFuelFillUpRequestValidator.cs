using FluentValidation;
using Logly.Application.FuelFillUps.DTOs;

namespace Logly.Application.FuelFillUps.Validators;

public sealed class UpdateFuelFillUpRequestValidator
    : AbstractValidator<UpdateFuelFillUpRequest>
{
    public UpdateFuelFillUpRequestValidator()
    {
        RuleFor(request => request.Date)
            .NotEqual(default(DateOnly))
            .WithMessage("A tankolás dátuma kötelező.");

        RuleFor(request => request.OdometerKm)
            .GreaterThan(0)
            .WithMessage("A kilométeróra állásának nagyobbnak kell lennie nullánál.");

        RuleFor(request => request.Liters)
            .GreaterThan(0)
            .WithMessage("A tankolt mennyiségnek nagyobbnak kell lennie nullánál.");

        RuleFor(request => request.TotalPrice)
            .GreaterThan(0)
            .WithMessage("A tankolás teljes árának nagyobbnak kell lennie nullánál.");

        RuleFor(request => request.Note)
            .MaximumLength(500)
            .WithMessage("A megjegyzés legfeljebb 500 karakter lehet.");
    }
}