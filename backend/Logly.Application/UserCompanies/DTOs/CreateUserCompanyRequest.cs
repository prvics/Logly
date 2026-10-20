using System.ComponentModel.DataAnnotations;

namespace Logly.Application.UserCompanies.DTOs;

public sealed class CreateUserCompanyRequest
{
    public Guid DeliveryCompanyId { get; set; }

    public DateOnly StartedOn { get; set; }

    [StringLength(
        500,
        ErrorMessage = "A megjegyzés legfeljebb 500 karakter lehet.")]
    public string? Note { get; set; }
}