using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface IContactRepository : IRepository<Contact>
{
    Task<IReadOnlyList<Contact>> SearchAsync(string term, CancellationToken ct = default);
}
