using System;
using System.Collections.Generic;
using Microsoft.eShopWeb.PublicApi;

namespace Microsoft.eShopWeb.PublicApi.OrderEndpoints;

public class CreateOrderResponse : BaseResponse
{
    public CreateOrderResponse(Guid correlationId) : base(correlationId) {}
    public CreateOrderResponse() {}
    public int OrderId { get; set; }
}

public class OrderDetailResponse : BaseResponse
{
    public OrderDetailResponse(Guid correlationId) : base(correlationId) {}
    public OrderDetailResponse() {}
    public OrderDetailDto Order { get; set; } = new();
}

public class ListOrdersResponse : BaseResponse
{
    public ListOrdersResponse(Guid correlationId) : base(correlationId) {}
    public ListOrdersResponse() {}
    public List<OrderSummaryDto> Orders { get; set; } = new();
}
