namespace Logly.Application.DeliveryCompanies.Exceptions;

public sealed class DeliveryCompanyAlreadyExistsException : Exception
{
    public DeliveryCompanyAlreadyExistsException()
        : base(
            "Ilyen nevű futárcég már létezik. " +
            "Válaszd ki a meglévő céget a listából.")
    {
    }
}