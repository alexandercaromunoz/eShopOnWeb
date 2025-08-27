using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications; // ensure spec namespace
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.OrderEndpoints;

[Authorize]
public class ListOrdersEndpoint : IEndpoint<IResult, ListOrdersRequest, IRepository<Order>>
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/orders", async (string buyerId, IRepository<Order> orderRepo) =>
        {
            return await HandleAsync(new ListOrdersRequest { BuyerId = buyerId }, orderRepo);
        }).Produces<ListOrdersResponse>()
          .WithTags("OrderEndpoints")
          .RequireAuthorization();
    }

    public async Task<IResult> HandleAsync(ListOrdersRequest request, IRepository<Order> orderRepository)
    {
        var spec = new OrdersByBuyerIdSpec(request.BuyerId);
        var orders = await orderRepository.ListAsync(spec);
        var response = new ListOrdersResponse(request.CorrelationId())
        {
            Orders = orders.Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                Total = o.Total(),
                Status = "Submitted"
            }).ToList()
        };
        return Results.Ok(response);
    }
}
