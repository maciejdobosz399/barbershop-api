namespace Barbershop.Infrastructure.Services;

public class EmailSettings
{
    public const string SectionName = "Email";

    public required string SmtpHost { get; init; }
    public required int SmtpPort { get; init; }
    public required string SmtpUsername { get; init; }
    public required string SmtpPassword { get; init; }
    public required string FromEmail { get; init; }
    public required string FromName { get; init; }
    public bool UseSsl { get; init; } = true;
}
