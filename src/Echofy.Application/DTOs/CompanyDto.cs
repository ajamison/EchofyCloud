namespace Echofy.Application.DTOs;

public class CompanyDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? TaxNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CompanyListItemDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public int InvoiceCount { get; set; }
}
