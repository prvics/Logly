using Logly.Application.Expenses.DTOs;
using Logly.Application.Expenses.Repositories;
using Logly.Application.Expenses.Services;
using Logly.Domain.Entities;
using Logly.Domain.Enums;
using Logly.Infrastructure.Expenses;
using Moq;

namespace Logly.Infrastructure.Tests;

public sealed class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _repositoryMock;
    private readonly ExpenseService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _expenseId = Guid.NewGuid();
    private readonly Guid _vehicleId = Guid.NewGuid();
    private readonly Guid _userCompanyId = Guid.NewGuid();

    public ExpenseServiceTests()
    {
        _repositoryMock = new Mock<IExpenseRepository>();
        _service = new ExpenseService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedExpenses()
    {
        var expenses = new List<Expense>
        {
            CreateExpense(
                id: _expenseId,
                userId: _userId,
                vehicleId: _vehicleId,
                userCompanyId: _userCompanyId,
                amount: 1_000m),

            CreateExpense(
                id: Guid.NewGuid(),
                userId: _userId,
                vehicleId: null,
                userCompanyId: null,
                amount: 2_500m)
        };

        _repositoryMock
            .Setup(repository => repository.GetAllByUserIdAsync(
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        var result = await _service.GetAllAsync(
            _userId,
            CancellationToken.None);

        Assert.Equal(2, result.Count);

        Assert.Equal(_expenseId, result[0].Id);
        Assert.Equal(_vehicleId, result[0].VehicleId);
        Assert.Equal(_userCompanyId, result[0].UserCompanyId);
        Assert.Equal(1_000m, result[0].Amount);
        Assert.Equal(ExpenseCategory.Parking, result[0].Category);

        Assert.Null(result[1].VehicleId);
        Assert.Null(result[1].UserCompanyId);
        Assert.Equal(2_500m, result[1].Amount);

        _repositoryMock.Verify(
            repository => repository.GetAllByUserIdAsync(
                _userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedExpenseExists_ReturnsMappedExpense()
    {
        var expense = CreateExpense(
            id: _expenseId,
            userId: _userId,
            vehicleId: _vehicleId,
            userCompanyId: _userCompanyId,
            amount: 1_000m);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _expenseId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        var result = await _service.GetByIdAsync(
            _userId,
            _expenseId,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(_expenseId, result.Id);
        Assert.Equal(_vehicleId, result.VehicleId);
        Assert.Equal(_userCompanyId, result.UserCompanyId);
        Assert.Equal(new DateOnly(2026, 9, 15), result.Date);
        Assert.Equal(1_000m, result.Amount);
        Assert.Equal(ExpenseCategory.Parking, result.Category);
        Assert.True(result.IsWorkRelated);
        Assert.Equal("Teszt kiadás", result.Note);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExpenseDoesNotExist_ReturnsNull()
    {
        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _expenseId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expense?)null);

        var result = await _service.GetByIdAsync(
            _userId,
            _expenseId,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidVehicleAndCompany_AddsExpenseAndReturnsSuccess()
    {
        var request = CreateValidRequest();

        SetupVehicleOwnership(true);
        SetupUserCompanyOwnership(true);
        SetupAddAndSave();

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Equal(ExpenseCreateStatus.Success, result.Status);
        Assert.NotNull(result.Expense);
        Assert.Null(result.ErrorMessage);
        
        Assert.Equal(_vehicleId, result.Expense.VehicleId);
        Assert.Equal(_userCompanyId, result.Expense.UserCompanyId);
        Assert.Equal(request.Date, result.Expense.Date);
        Assert.Equal(2_500m, result.Expense.Amount);
        Assert.Equal(ExpenseCategory.Parking, result.Expense.Category);
        Assert.True(result.Expense.IsWorkRelated);
        Assert.Equal("Teszt kiadás", result.Expense.Note);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<Expense>(expense =>
                    expense.UserId == _userId &&
                    expense.VehicleId == _vehicleId &&
                    expense.UserCompanyId == _userCompanyId &&
                    expense.Date == request.Date &&
                    expense.Amount == request.Amount &&
                    expense.Category == request.Category &&
                    expense.IsWorkRelated == request.IsWorkRelated &&
                    expense.Note == request.Note),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithOnlyRequiredData_AddsExpenseWithoutOwnershipChecks()
    {
        var request = new CreateExpenseRequest
        {
            VehicleId = null,
            UserCompanyId = null,
            Date = new DateOnly(2026, 9, 16),
            Amount = 1_000m,
            Category = ExpenseCategory.Other,
            IsWorkRelated = false,
            Note = null
        };

        SetupAddAndSave();

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Equal(ExpenseCreateStatus.Success, result.Status);
        Assert.NotNull(result.Expense);
        Assert.Null(result.Expense.VehicleId);
        Assert.Null(result.Expense.UserCompanyId);
        Assert.Equal(1_000m, result.Expense.Amount);
        Assert.False(result.Expense.IsWorkRelated);

        _repositoryMock.Verify(
            repository => repository.VehicleBelongsToUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.UserCompanyBelongsToUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Expense>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateAsync_WithZeroOrNegativeAmount_ReturnsValidationFailed(
        decimal amount)
    {
        var request = CreateValidRequest();
        request = new CreateExpenseRequest
        {
            VehicleId = request.VehicleId,
            UserCompanyId = request.UserCompanyId,
            Date = request.Date,
            Amount = amount,
            Category = request.Category,
            IsWorkRelated = request.IsWorkRelated,
            Note = request.Note
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Equal(ExpenseCreateStatus.ValidationFailed, result.Status);
        Assert.Null(result.Expense);
        Assert.NotNull(result.ErrorMessage);

        VerifyNoWriteOperations();
        VerifyNoOwnershipChecks();
    }

    [Fact]
    public async Task CreateAsync_WithAmountAboveMaximum_ReturnsValidationFailed()
    {
        var request = new CreateExpenseRequest
        {
            VehicleId = _vehicleId,
            UserCompanyId = _userCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 10_000_000_000m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = "Túl nagy összeg"
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Equal(ExpenseCreateStatus.ValidationFailed, result.Status);

        VerifyNoWriteOperations();
        VerifyNoOwnershipChecks();
    }

    [Fact]
    public async Task CreateAsync_WithInvalidCategory_ReturnsValidationFailed()
    {
        var request = new CreateExpenseRequest
        {
            VehicleId = _vehicleId,
            UserCompanyId = _userCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 1_000m,
            Category = (ExpenseCategory)12345,
            IsWorkRelated = true,
            Note = "Érvénytelen kategória"
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Equal(ExpenseCreateStatus.ValidationFailed, result.Status);

        VerifyNoWriteOperations();
        VerifyNoOwnershipChecks();
    }

    [Fact]
    public async Task CreateAsync_WithNoteLongerThan500Characters_ReturnsValidationFailed()
    {
        var request = new CreateExpenseRequest
        {
            VehicleId = _vehicleId,
            UserCompanyId = _userCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 1_000m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = new string('A', 1001)
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Equal(ExpenseCreateStatus.ValidationFailed, result.Status);

        VerifyNoWriteOperations();
        VerifyNoOwnershipChecks();
    }

    [Fact]
    public async Task CreateAsync_WhenVehicleIsNotOwned_ReturnsVehicleNotOwnedByUser()
    {
        var request = CreateValidRequest();

        SetupVehicleOwnership(false);

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Equal(ExpenseCreateStatus.VehicleNotOwnedByUser, result.Status);
        Assert.Null(result.Expense);

        _repositoryMock.Verify(
            repository => repository.UserCompanyBelongsToUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifyNoWriteOperations();
    }

    [Fact]
    public async Task CreateAsync_WhenUserCompanyIsNotOwned_ReturnsUserCompanyNotOwnedByUser()
    {
        var request = CreateValidRequest();

        SetupVehicleOwnership(true);
        SetupUserCompanyOwnership(false);

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Equal(
            ExpenseCreateStatus.UserCompanyNotOwnedByUser,
            result.Status);

        Assert.Null(result.Expense);
        VerifyNoWriteOperations();
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesExpenseAndReturnsSuccess()
    {
        var existingExpense = CreateExpense(
            id: _expenseId,
            userId: _userId,
            vehicleId: _vehicleId,
            userCompanyId: _userCompanyId,
            amount: 1_000m);

        var updatedVehicleId = Guid.NewGuid();
        var updatedUserCompanyId = Guid.NewGuid();

        var request = new UpdateExpenseRequest
        {
            VehicleId = updatedVehicleId,
            UserCompanyId = updatedUserCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 3_500m,
            Category = ExpenseCategory.Equipment,
            IsWorkRelated = false,
            Note = "Módosított kiadás"
        };

        _repositoryMock
            .Setup(repository => repository.VehicleBelongsToUserAsync(
                updatedVehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                updatedUserCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _expenseId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingExpense);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(
            _userId,
            _expenseId,
            request,
            CancellationToken.None);

        Assert.Equal(ExpenseCreateStatus.Success, result.Status);
        Assert.NotNull(result.Expense);

        Assert.Equal(updatedVehicleId, existingExpense.VehicleId);
        Assert.Equal(updatedUserCompanyId, existingExpense.UserCompanyId);
        Assert.Equal(new DateOnly(2026, 9, 16), existingExpense.Date);
        Assert.Equal(3_500m, existingExpense.Amount);
        Assert.Equal(ExpenseCategory.Equipment, existingExpense.Category);
        Assert.False(existingExpense.IsWorkRelated);
        Assert.Equal("Módosított kiadás", existingExpense.Note);
        Assert.NotNull(existingExpense.UpdatedAtUtc);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenExpenseDoesNotExist_ReturnsNotFoundWithoutSaving()
    {
        var request = CreateValidUpdateRequest();

        SetupVehicleOwnership(true);
        SetupUserCompanyOwnership(true);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _expenseId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expense?)null);

        var result = await _service.UpdateAsync(
            _userId,
            _expenseId,
            request,
            CancellationToken.None);

        Assert.Equal(ExpenseCreateStatus.NotFound, result.Status);
        Assert.Null(result.Expense);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenVehicleIsNotOwned_ReturnsVehicleNotOwnedByUser()
    {
        var request = CreateValidUpdateRequest();

        SetupVehicleOwnership(false);

        var result = await _service.UpdateAsync(
            _userId,
            _expenseId,
            request,
            CancellationToken.None);

        Assert.Equal(ExpenseCreateStatus.VehicleNotOwnedByUser, result.Status);

        _repositoryMock.Verify(
            repository => repository.GetByIdAndUserIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifyNoWriteOperations();
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedExpenseExists_RemovesAndReturnsTrue()
    {
        var expense = CreateExpense(
            id: _expenseId,
            userId: _userId,
            vehicleId: _vehicleId,
            userCompanyId: _userCompanyId,
            amount: 1_000m);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _expenseId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(
            _userId,
            _expenseId,
            CancellationToken.None);

        Assert.True(result);

        _repositoryMock.Verify(
            repository => repository.Remove(expense),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenExpenseDoesNotExist_ReturnsFalseWithoutDeleting()
    {
        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _expenseId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expense?)null);

        var result = await _service.DeleteAsync(
            _userId,
            _expenseId,
            CancellationToken.None);

        Assert.False(result);

        _repositoryMock.Verify(
            repository => repository.Remove(
                It.IsAny<Expense>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private CreateExpenseRequest CreateValidRequest()
    {
        return new CreateExpenseRequest
        {
            VehicleId = _vehicleId,
            UserCompanyId = _userCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 2_500m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = "Teszt kiadás"
        };
    }

    private UpdateExpenseRequest CreateValidUpdateRequest()
    {
        return new UpdateExpenseRequest
        {
            VehicleId = _vehicleId,
            UserCompanyId = _userCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 3_500m,
            Category = ExpenseCategory.Equipment,
            IsWorkRelated = false,
            Note = "Frissített teszt kiadás"
        };
    }

    private static Expense CreateExpense(
        Guid id,
        Guid userId,
        Guid? vehicleId,
        Guid? userCompanyId,
        decimal amount)
    {
        return new Expense
        {
            Id = id,
            UserId = userId,
            VehicleId = vehicleId,
            UserCompanyId = userCompanyId,
            Date = new DateOnly(2026, 9, 15),
            Amount = amount,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = "Teszt kiadás"
        };
    }

    private void SetupVehicleOwnership(bool isOwned)
    {
        _repositoryMock
            .Setup(repository => repository.VehicleBelongsToUserAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isOwned);
    }

    private void SetupUserCompanyOwnership(bool isOwned)
    {
        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isOwned);
    }

    private void SetupAddAndSave()
    {
        _repositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<Expense>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void VerifyNoWriteOperations()
    {
        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Expense>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void VerifyNoOwnershipChecks()
    {
        _repositoryMock.Verify(
            repository => repository.VehicleBelongsToUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.UserCompanyBelongsToUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}