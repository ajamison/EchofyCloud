namespace Echofy.Domain.Entities;

public class RewardProgram
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Client Client { get; set; } = null!;
    public Company? Company { get; set; }
    public ICollection<RewardTier> Tiers { get; set; } = [];
}
