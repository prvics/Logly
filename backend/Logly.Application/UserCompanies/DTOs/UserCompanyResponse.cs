namespace Logly.Application.UserCompanies.DTOs;

public sealed class UserCompanyResponse
{
    public Guid Id { get; set; }

    public Guid DeliveryCompanyId { get; set; }

    public string DeliveryCompanyName { get; set; } = string.Empty;

    public bool IsSystemDefined { get; set; }

    public DateOnly StartedOn { get; set; }

    public DateOnly? EndedOn { get; set; }

    public bool IsActive { get; set; }

    public string? Note { get; set; }
}