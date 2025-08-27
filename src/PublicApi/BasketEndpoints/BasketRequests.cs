using System; // Guid
using System.Collections.Generic; // Dictionary
using Microsoft.eShopWeb.PublicApi;

namespace Microsoft.eShopWeb.PublicApi.BasketEndpoints;

public class GetBasketRequest : BaseRequest
{
    public string BuyerId { get; set; } = string.Empty;
}

public class ModifyBasketItemRequest : BaseRequest
{
    public string BuyerId { get; set; } = string.Empty;
    public int CatalogItemId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateBasketQuantitiesRequest : BaseRequest
{
    public int BasketId { get; set; }
    public Dictionary<int,int> Quantities { get; set; } = new(); // basketItemId => quantity
}
