namespace Echofy.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string toName, string subject, string htmlBody, CancellationToken ct = default);
}
