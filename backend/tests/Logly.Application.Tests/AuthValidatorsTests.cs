using Logly.Application.Auth.DTOs;
using Logly.Application.Auth.Validators;

namespace Logly.Application.Tests;

public sealed class AuthValidatorsTests
{
    private readonly LoginRequestValidator _loginValidator = new();
    private readonly RegisterRequestValidator _registerValidator = new();

    [Fact]
    public void Login_WithValidRequest_IsValid()
    {
        var request = new LoginRequest
        {
            Email = "futar@example.com",
            Password = "BiztonsagosJelszo123"
        };

        var result = _loginValidator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nem-egy-email")]
    [InlineData("futar@")]
    public void Login_WhenEmailIsMissingOrInvalid_IsInvalid(
        string email)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = "BiztonsagosJelszo123"
        };

        var result = _loginValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(LoginRequest.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Login_WhenPasswordIsMissing_IsInvalid(
        string password)
    {
        var request = new LoginRequest
        {
            Email = "futar@example.com",
            Password = password
        };

        var result = _loginValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(LoginRequest.Password));
    }

    [Fact]
    public void Register_WithValidRequest_IsValid()
    {
        var request = new RegisterRequest
        {
            Email = "futar@example.com",
            Password = "BiztonsagosJelszo123",
            DisplayName = "Péter"
        };

        var result = _registerValidator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("rossz-email")]
    public void Register_WhenEmailIsInvalid_IsInvalid(
        string email)
    {
        var request = new RegisterRequest
        {
            Email = email,
            Password = "BiztonsagosJelszo123",
            DisplayName = "Péter"
        };

        var result = _registerValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(RegisterRequest.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567")]
    public void Register_WhenPasswordIsMissingOrTooShort_IsInvalid(
        string password)
    {
        var request = new RegisterRequest
        {
            Email = "futar@example.com",
            Password = password,
            DisplayName = "Péter"
        };

        var result = _registerValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void Register_WhenDisplayNameIsWhitespace_IsInvalid()
    {
        var request = new RegisterRequest
        {
            Email = "futar@example.com",
            Password = "BiztonsagosJelszo123",
            DisplayName = "   "
        };

        var result = _registerValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(RegisterRequest.DisplayName));
    }

    [Fact]
    public void Register_WhenDisplayNameIsLongerThan100Characters_IsInvalid()
    {
        var request = new RegisterRequest
        {
            Email = "futar@example.com",
            Password = "BiztonsagosJelszo123",
            DisplayName = new string('a', 101)
        };

        var result = _registerValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(RegisterRequest.DisplayName));
    }

    [Fact]
    public void Register_WhenDisplayNameIsNull_IsInvalid()
    {
        var request = new RegisterRequest
        {
            Email = "futar@example.com",
            Password = "BiztonsagosJelszo123",
            DisplayName = null!
        };

        var result = _registerValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                     nameof(RegisterRequest.DisplayName));
    }
}