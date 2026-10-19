namespace Logly.Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "Logly";

    public string Security { get; set; } = "StartTls";

    public string? Username { get; set; }

    public string? Password { get; set; }
}