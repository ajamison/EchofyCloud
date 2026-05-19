using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(int customerId, CancellationToken ct = default);
    Task<Order?> GetWithItemsAsync(int id, CancellationToken ct = default);
    Task<int> CountNewOrdersAsync(CancellationToken ct = default);
    Task<int> CountOnHoldAsync(CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
}
