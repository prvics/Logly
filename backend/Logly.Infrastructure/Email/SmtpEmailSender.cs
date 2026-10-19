using Logly.Application.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Logly.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;

    public SmtpEmailSender(SmtpOptions options)
    {
        _options = options;
    }

    public async Task SendAsync(
        string recipientEmail,
        string subject,
        string textBody,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            throw new InvalidOperationException(
                "SMTP host is not configured.");
        }

        if (_options.Port is < 1 or > 65535)
        {
            throw new InvalidOperationException(
                "SMTP port must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            throw new InvalidOperationException(
                "SMTP sender email is not configured.");
        }

        var security = _options.Security switch
        {
            "None" => SecureSocketOptions.None,
            "StartTls" => SecureSocketOptions.StartTls,
            "SslOnConnect" => SecureSocketOptions.SslOnConnect,
            _ => throw new InvalidOperationException(
                "SMTP security must be None, StartTls or SslOnConnect.")
        };

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _options.FromName,
                _options.FromEmail));

        message.To.Add(
            MailboxAddress.Parse(recipientEmail));

        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = textBody
        };

        using var client = new SmtpClient();

        client.Timeout = 15_000;

        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            security,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            await client.AuthenticateAsync(
                _options.Username,
                _options.Password ?? string.Empty,
                cancellationToken);
        }

        await client.SendAsync(
            message,
            cancellationToken);

        await client.DisconnectAsync(
            true,
            cancellationToken);
    }
}