namespace Echofy.Application.DTOs;

public class DashboardStatsDto
{
    public int OutOfStockProducts { get; set; }
    public int NewCustomers { get; set; }
    public decimal PercentageDiscountShare { get; set; }
    public decimal FixedCartDiscountShare { get; set; }
    public decimal FixedProductDiscountShare { get; set; }
}
