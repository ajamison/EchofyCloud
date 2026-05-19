namespace Echofy.MobileApp.Models;

public class DiscountOfferDto
{
    public string   Name          { get; set; } = string.Empty;
    public string   DiscountType  { get; set; } = string.Empty;
    public decimal  DiscountValue { get; set; }
    public DateTime EndDate       { get; set; }
    public bool     IsActive      { get; set; }
}

public class ProductImageDto
{
    public int    Id       { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool   IsMain   { get; set; }
    public string Url      { get; set; } = string.Empty;
}

public class ProductDto
{
    public int     Id                  { get; set; }
    public string  Name                { get; set; } = string.Empty;
    public string  Description         { get; set; } = string.Empty;
    public decimal Price               { get; set; }
    public decimal EffectivePrice      { get; set; }
    public int     StockQuantity       { get; set; }
    public string? ManufacturerUpc     { get; set; }
    public string? Sku                 { get; set; }
    public string? ManufacturerName    { get; set; }
    public string? ManufacturerWebsite { get; set; }
    public string? UnitOfMeasureName   { get; set; }
    public string? ImageUrl            { get; set; }
    public string  ShortId             { get; set; } = string.Empty;
    public bool    IsActive            { get; set; }

    public List<ProductImageDto> Images      { get; set; } = [];
    public DiscountOfferDto?     ActiveOffer { get; set; }
}
