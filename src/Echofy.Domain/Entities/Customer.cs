using Echofy.Domain.Interfaces;

namespace Echofy.Domain.Entities;

public class Customer : IAuditable
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public Client? Client { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }
    public string? Notes { get; set; }

    public Address Address { get; set; } = new();

    public ICollection<Order> Orders { get; set; } = [];
}
