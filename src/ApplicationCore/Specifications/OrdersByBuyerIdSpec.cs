using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Specifications;

public sealed class OrdersByBuyerIdSpec : Specification<Order>
{
    public OrdersByBuyerIdSpec(string buyerId)
    {
        Query.Where(o => o.BuyerId == buyerId)
             .OrderByDescending(o => o.OrderDate)
             .Include(o => o.OrderItems);
    }
}
