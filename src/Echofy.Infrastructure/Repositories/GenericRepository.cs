using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class GenericRepository<T>(ApplicationDbContext db) : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Db = db;
    protected readonly DbSet<T> Set = db.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await Set.FindAsync([id], ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await Set.ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public void Update(T entity) => Set.Update(entity);

    public void Delete(T entity) => Set.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await Db.SaveChangesAsync(ct);
}
