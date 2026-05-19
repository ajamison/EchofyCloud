using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class ContactRepository(ApplicationDbContext db)
    : GenericRepository<Contact>(db), IContactRepository
{
    public async Task<IReadOnlyList<Contact>> SearchAsync(string term, CancellationToken ct = default) =>
        await Db.Contacts
            .Where(c => c.FirstName.Contains(term) || c.LastName.Contains(term) || c.Email.Contains(term))
            .ToListAsync(ct);
}
