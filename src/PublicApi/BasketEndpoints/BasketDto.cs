using System.Collections.Generic;
using System.Linq; // added

namespace Microsoft.eShopWeb.PublicApi.BasketEndpoints;

public class BasketDto
{
    public int Id { get; set; }
    public string BuyerId { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}

public class BasketItemDto
{
    public int Id { get; set; }
    public int CatalogItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string PictureUrl { get; set; } = string.Empty;
}
