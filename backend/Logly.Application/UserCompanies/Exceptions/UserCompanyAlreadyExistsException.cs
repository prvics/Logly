namespace Logly.Application.UserCompanies.Exceptions;

public sealed class UserCompanyAlreadyExistsException : Exception
{
    public UserCompanyAlreadyExistsException()
        : base(
            "Ez a futárcég már szerepel a saját cégeid között. " +
            "Ha a kapcsolat inaktív, azt kell újraaktiválni.")
    {
    }
}