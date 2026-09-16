using Logly.Application.Expenses.DTOs;
using Logly.Application.Expenses.Validators;
using Logly.Domain.Enums;

namespace Logly.Application.Tests;

public sealed class ExpenseValidatorsTests
{
    private readonly CreateExpenseRequestValidator _createValidator = new();
    private readonly UpdateExpenseRequestValidator _updateValidator = new();

    [Fact]
    public void Create_WithValidRequest_IsValid()
    {
        var result = _createValidator.Validate(CreateValidRequest());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2500)]
    public void Create_WhenAmountIsNotGreaterThanZero_IsInvalid(
        decimal amount)
    {
        var request = new CreateExpenseRequest
        {
            VehicleId = Guid.NewGuid(),
            UserCompanyId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            Amount = amount,
            Category = ExpenseCategory.Other,
            IsWorkRelated = true,
            Note = "Telefon tartó"
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateExpenseRequest.Amount));
    }

    [Fact]
    public void Create_WhenVehicleIdIsEmpty_IsInvalid()
    {
        var request = new CreateExpenseRequest
        {
            VehicleId = Guid.Empty,
            UserCompanyId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            Amount = 2500m,
            Category = ExpenseCategory.Other,
            IsWorkRelated = true
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateExpenseRequest.VehicleId));
    }

    [Fact]
    public void Create_WhenUserCompanyIdIsEmpty_IsInvalid()
    {
        var request = new CreateExpenseRequest
        {
            VehicleId = Guid.NewGuid(),
            UserCompanyId = Guid.Empty,
            Date = new DateOnly(2025, 9, 10),
            Amount = 2500m,
            Category = ExpenseCategory.Other,
            IsWorkRelated = true
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateExpenseRequest.UserCompanyId));
    }

    [Fact]
    public void Create_WhenNoteIsLongerThan500Characters_IsInvalid()
    {
        var request = new CreateExpenseRequest
        {
            VehicleId = Guid.NewGuid(),
            UserCompanyId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            Amount = 2500m,
            Category = ExpenseCategory.Other,
            IsWorkRelated = true,
            Note = new string('a', 501)
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateExpenseRequest.Note));
    }

    [Fact]
    public void Update_WithValidRequest_IsValid()
    {
        var request = new UpdateExpenseRequest
        {
            VehicleId = Guid.NewGuid(),
            UserCompanyId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            Amount = 2500m,
            Category = ExpenseCategory.Other,
            IsWorkRelated = false,
            Note = "Karbantartás"
        };

        var result = _updateValidator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_WhenAmountIsZero_IsInvalid()
    {
        var request = new UpdateExpenseRequest
        {
            VehicleId = Guid.NewGuid(),
            UserCompanyId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            Amount = 0m,
            Category = ExpenseCategory.Other,
            IsWorkRelated = false
        };

        var result = _updateValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateExpenseRequest.Amount));
    }

    private static CreateExpenseRequest CreateValidRequest()
    {
        return new CreateExpenseRequest
        {
            VehicleId = Guid.NewGuid(),
            UserCompanyId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            Amount = 2500m,
            Category = ExpenseCategory.Other,
            IsWorkRelated = true,
            Note = "Gumi"
        };
    }
    
    [Fact]
    public void UpdateExpense_WhenNoteIsNull_IsValid()
    {
        var request = new UpdateExpenseRequest
        {
            Date = new DateOnly(2026, 9, 16),
            Amount = 1_000m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = null
        };

        var result = _updateValidator.Validate(request);

        Assert.True(result.IsValid);
    }
}