namespace Echofy.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;     // Created / Updated / Deleted
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }                 // JSON snapshot before change
    public string? NewValues { get; set; }                 // JSON snapshot after change
    public string? ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
