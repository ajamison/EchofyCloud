namespace Echofy.Application.Interfaces;

public record SendThankYouRequest(int InvoiceId, int? ClientId, string? CustomMessage, string BaseUrl);

public interface IThankYouNoteService
{
    Task<bool> SendAsync(SendThankYouRequest req, CancellationToken ct = default);
}
