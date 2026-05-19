namespace Echofy.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime JoinedDate { get; set; }
    public string? Notes { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<ReviewDto> Reviews { get; set; } = [];
}

public class CustomerListItemDto
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime JoinedDate { get; set; }
}
