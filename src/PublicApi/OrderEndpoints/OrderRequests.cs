using Microsoft.eShopWeb.PublicApi;

namespace Microsoft.eShopWeb.PublicApi.OrderEndpoints;

public class CreateOrderRequest : BaseRequest
{
    public int BasketId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class GetOrderRequest : BaseRequest
{
    public int OrderId { get; set; }
}

public class ListOrdersRequest : BaseRequest
{
    public string BuyerId { get; set; } = string.Empty;
}
