using System; // Guid
using Microsoft.eShopWeb.PublicApi;

namespace Microsoft.eShopWeb.PublicApi.BasketEndpoints;

public class BasketResponse : BaseResponse
{
    public BasketResponse(Guid correlationId) : base(correlationId) {}
    public BasketResponse() {}
    public BasketDto Basket { get; set; } = new();
}
