namespace Logly.Application.Email;

public interface IEmailSender
{
    Task SendAsync(
        string recipientEmail,
        string subject,
        string textBody,
        CancellationToken cancellationToken);
}