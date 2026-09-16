using System.Net.Http.Headers;

namespace Logly.Api.IntegrationTests.Infrastructure;

public abstract class EndpointTestsBase
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected EndpointTestsBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected CustomWebApplicationFactory Factory { get; }

    protected HttpClient Client { get; }

    public virtual async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected void AuthenticateAs(Guid userId)
    {
        Client.DefaultRequestHeaders.Authorization = null;

        Client.DefaultRequestHeaders.Remove(
            TestAuthHandler.UserIdHeaderName);

        Client.DefaultRequestHeaders.Add(
            TestAuthHandler.UserIdHeaderName,
            userId.ToString());
    }

    protected void ClearAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        Client.DefaultRequestHeaders.Remove(
            TestAuthHandler.UserIdHeaderName);
    }
}