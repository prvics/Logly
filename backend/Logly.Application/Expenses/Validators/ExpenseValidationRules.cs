using FluentValidation;
using Logly.Domain.Enums;

namespace Logly.Application.Expenses.Validators;

internal static class ExpenseValidationRules
{
    internal static IRuleBuilderOptions<T, Guid?> ValidOptionalId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage($"{fieldName} nem lehet üres azonosító.");
    }

    internal static void Apply<T>(AbstractValidator<T> validator)
        where T : class
    {
    }
}