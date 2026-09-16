using System.Net;
using Logly.Api.IntegrationTests.Infrastructure;

namespace Logly.Api.IntegrationTests;

public sealed class AuthorizationEndpointsTests
    : EndpointTestsBase
{
    public AuthorizationEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetVehicles_WithoutAuthentication_ReturnsUnauthorized()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/vehicles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboardSummary_WithoutAuthentication_ReturnsUnauthorized()
    {
        ClearAuthentication();

        var response = await Client.GetAsync(
            "/api/dashboard/summary?from=2026-09-01&to=2026-09-30");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}