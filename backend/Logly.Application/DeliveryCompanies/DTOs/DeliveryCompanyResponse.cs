namespace Logly.Application.DeliveryCompanies.DTOs;

public sealed class DeliveryCompanyResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsSystemDefined { get; set; }
}