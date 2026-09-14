using Logly.Domain.Common;

namespace Logly.Domain.Entities;

public class DeliveryCompany : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public bool IsSystemDefined { get; set; }

    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
}