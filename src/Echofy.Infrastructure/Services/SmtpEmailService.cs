using System.Net;
using System.Net.Mail;
using Echofy.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Echofy.Infrastructure.Services;

public class SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendAsync(string to, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        var section = configuration.GetSection("Email");
        var host    = section["Smtp:Host"];

        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogInformation(
                "Email not sent (SMTP not configured). To: {To} | Subject: {Subject}", to, subject);
            return;
        }

        var port     = int.Parse(section["Smtp:Port"] ?? "587");
        var username = section["Smtp:Username"] ?? string.Empty;
        var password = section["Smtp:Password"] ?? string.Empty;
        var from     = section["From"] ?? username;
        var fromName = section["FromName"] ?? "Echofy";

#pragma warning disable CA1416, SYSLIB0006
        using var client = new SmtpClient(host, port)
        {
            EnableSsl   = bool.Parse(section["Smtp:EnableSsl"] ?? "true"),
            Credentials = new NetworkCredential(username, password),
        };

        var msg = new MailMessage
        {
            From       = new MailAddress(from, fromName),
            Subject    = subject,
            Body       = htmlBody,
            IsBodyHtml = true,
        };
        msg.To.Add(new MailAddress(to, toName));

        await client.SendMailAsync(msg, ct);
#pragma warning restore CA1416, SYSLIB0006

        logger.LogInformation("Email sent. To: {To} | Subject: {Subject}", to, subject);
    }
}
