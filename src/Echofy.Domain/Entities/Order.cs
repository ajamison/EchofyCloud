using Echofy.Domain.Enums;
using Echofy.Domain.Interfaces;

namespace Echofy.Domain.Entities;

public class Order : IAuditable
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.Unfulfilled;
    public DeliveryType DeliveryType { get; set; } = DeliveryType.Standard;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = [];
}
