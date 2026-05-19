using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class DashboardService(
    IProductRepository products,
    ICustomerRepository customers) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var outOfStock = await products.CountOutOfStockAsync(ct);

        var allCustomers = await customers.GetAllAsync(null, null, ct);
        var newCustomers = allCustomers.Count(c => c.JoinedDate >= DateTime.UtcNow.AddDays(-7));

        return new DashboardStatsDto
        {
            OutOfStockProducts = outOfStock,
            NewCustomers = newCustomers,
            PercentageDiscountShare = 72m,
            FixedCartDiscountShare = 18m,
            FixedProductDiscountShare = 10m
        };
    }
}
