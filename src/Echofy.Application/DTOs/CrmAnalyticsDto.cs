namespace Echofy.Application.DTOs;

public class CrmAnalyticsDto
{
    public int TotalLeads { get; set; }
    public int TotalDeals { get; set; }
    public decimal TotalPipelineValue { get; set; }
    public decimal ConversionRate { get; set; }
    public Dictionary<string, int> DealsByStage { get; set; } = [];
    public Dictionary<string, int> LeadsByStatus { get; set; } = [];
}
