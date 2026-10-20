using System.ComponentModel.DataAnnotations;

namespace Logly.Application.UserCompanies.DTOs;

public sealed class UpdateUserCompanyRequest
{
    public DateOnly StartedOn { get; set; }

    public DateOnly? EndedOn { get; set; }

    public bool IsActive { get; set; }

    [StringLength(
        500,
        ErrorMessage = "A megjegyzés legfeljebb 500 karakter lehet.")]
    public string? Note { get; set; }
}