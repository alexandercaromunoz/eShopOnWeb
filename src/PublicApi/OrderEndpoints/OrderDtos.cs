using System;
using System.Collections.Generic;

namespace Microsoft.eShopWeb.PublicApi.OrderEndpoints;

public class OrderSummaryDto
{
    public int Id { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pending"; // placeholder
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Units { get; set; }
    public string PictureUrl { get; set; } = string.Empty;
}

public class OrderDetailDto : OrderSummaryDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}
