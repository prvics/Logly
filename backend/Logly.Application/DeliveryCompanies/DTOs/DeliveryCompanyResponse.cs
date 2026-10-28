namespace Logly.Application.DeliveryCompanies.DTOs;

public sealed class DeliveryCompanyResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool IsSystemDefined { get; init; }

    public Guid? OwnerId { get; init; }
    
    public string? OwnerDisplayName { get; init; }

    public string? OwnerEmail { get; init; }
}