using Barbershop.Application.Abstractions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Barbershop.Infrastructure.Services;

public class EmailService(
    IOptions<EmailSettings> emailSettings,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = emailSettings.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;

        email.Body = message.IsHtml
            ? new BodyBuilder { HtmlBody = message.Body }.ToMessageBody()
            : new TextPart("plain") { Text = message.Body };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, _settings.UseSsl, cancellationToken);
            await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword, cancellationToken);
            await client.SendAsync(email, cancellationToken);

            logger.LogInformation(
                "Email sent successfully to {To} with subject '{Subject}'",
                message.To,
                message.Subject);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send email to {To} with subject '{Subject}'",
                message.To,
                message.Subject);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
