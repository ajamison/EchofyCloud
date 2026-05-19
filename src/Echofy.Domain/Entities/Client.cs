namespace Echofy.Domain.Entities;

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    // Module flags
    public bool HasECommerce { get; set; }
    public bool HasCrm { get; set; }
    public bool HasKanban { get; set; }
    public bool HasCalendar { get; set; }
    public bool HasChat { get; set; }

    public bool AllowCompanyRewardOverride { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>8-char hex string (4 bytes) used as the NTAG PWD_AUTH password. Null = not configured.</summary>
    public string? NfcCardPassword { get; set; }
}
