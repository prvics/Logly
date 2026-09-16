using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Logly.Api.Validation;

public static class ValidationExtensions
{
    public static BadRequestObjectResult ToBadRequest(
        this ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        return new BadRequestObjectResult(new
        {
            message = "Érvénytelen kérés.",
            errors
        });
    }
}