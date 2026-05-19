using Echofy.Domain.Entities;
using Echofy.Domain.Enums;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class OrderRepository(ApplicationDbContext db)
    : GenericRepository<Order>(db), IOrderRepository
{
    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(int customerId, CancellationToken ct = default) =>
        await Db.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<Order?> GetWithItemsAsync(int id, CancellationToken ct = default) =>
        await Db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<int> CountNewOrdersAsync(CancellationToken ct = default) =>
        await Db.Orders.CountAsync(o => o.FulfillmentStatus == FulfillmentStatus.Unfulfilled, ct);

    public async Task<int> CountOnHoldAsync(CancellationToken ct = default) =>
        await Db.Orders.CountAsync(o => o.PaymentStatus == PaymentStatus.Pending
            && o.FulfillmentStatus == FulfillmentStatus.Unfulfilled, ct);

    public async Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default) =>
        await Db.Orders.Where(o => o.PaymentStatus == PaymentStatus.Paid).SumAsync(o => o.Total, ct);
}
