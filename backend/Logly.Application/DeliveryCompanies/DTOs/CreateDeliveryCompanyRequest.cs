using System.ComponentModel.DataAnnotations;

namespace Logly.Application.DeliveryCompanies.DTOs;

public sealed class CreateDeliveryCompanyRequest
{
    [Required(ErrorMessage = "A futárcég neve kötelező.")]
    [StringLength(
        100,
        ErrorMessage = "A futárcég neve legfeljebb 100 karakter lehet.")]
    public string Name { get; set; } = string.Empty;

    public bool IsSystemDefined { get; set; }
}